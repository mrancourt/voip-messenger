using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Couchbase.Lite;
using Android.Util;
using Android.Views.InputMethods;
using Android.Views;
using Utilities;
using Newtonsoft.Json;

namespace messenger
{
	[Activity (WindowSoftInputMode = SoftInput.AdjustResize, Label = "@string/ConversationActivity.label", Theme = "@style/Theme.Base")]			
	public class ConversationActivity : Activity
	{

		static readonly string Tag = "conversations";

		Query Query { get; set; }
		LiveQuery LiveQuery { get; set; }
		Database Database { get; set; }
		Replication Pull { get; set; }
		Replication Push { get; set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			RequestWindowFeature(WindowFeatures.IndeterminateProgress);

			// Load contact infos
			long contactId = Intent.GetLongExtra("Id", -1);
			Contact contact = new Contact ().GetContactById(contactId, this);
			Log.Verbose("ConversationActivity", contact.ToString());

			// Instantiante Sms
			SMS sms = new SMS(); 
			sms.Target =  contact.NormalizedNumber.ToDigitsOnly ();

			// Get database instance
			Database = Manager.SharedInstance.GetDatabase(Tag.ToLower() + "_" + sms.Target);

			// Get previous messages
			Query = Message.GetQuery(Database);
			Query.Completed += (sender, e) => 
				Log.Verbose("ConversationActivity", e.ErrorInfo.ToString() ?? e.Rows.ToString());
			LiveQuery = Query.ToLiveQuery();
		
			SetContentView (Resource.Layout.Conversation);

			// Activate Back button in Action Bar
			ActionBar.SetHomeButtonEnabled(true);
			ActionBar.SetDisplayHomeAsUpEnabled(true);

			// Rethrive layout fields
			TextView newMessageText = FindViewById<TextView> (Resource.Id.txtMessageBox);
			FloatingActionButton sendMessageButton = FindViewById<FloatingActionButton> (Resource.Id.btnSendMessage);
			LinearLayout layout = (LinearLayout) FindViewById(Resource.Id.mainLinearLayout);
			ListView listView = (ListView) FindViewById(Resource.Id.listViewMessages);

			// Set ActionBar to contact name
			this.Title = contact.DisplayName;

			sendMessageButton.Click += (sender, e) => {
				//MessageHandler smsHandler = new MessageHandler();
				//smsHandler.sendSMS();

				sms.Message = newMessageText.Text;

				Log.Verbose(Tag, JsonConvert.SerializeObject(sms));

				SendSms(sms);
				newMessageText.Text = "";

				Log.Verbose("ConversationActivity","Message Sent");

			};

			Database.Changed += (sender, e) => {
				var changes = e.Changes.ToList();
				foreach (DocumentChange change in changes) {
					Log.Verbose(Tag, "Document " + change.DocumentId + " changed");
				}
			};
				
			listView.Adapter = new ListLiveQueryAdapter(this, LiveQuery);

		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
			case Android.Resource.Id.Home:
				Finish();
				return true;

			default:
				return base.OnOptionsItemSelected(item);
			}
		}

		protected override void OnResume()
		{
			base.OnResume(); // Always call the superclass first.

			UpdateSync();
		}

		private void UpdateSync()
		{
			if (Database == null)
				return;

			var preferences = PreferenceManager.GetDefaultSharedPreferences(this);
			var syncUrl = preferences.GetString("sync-gateway-url", null);

			ForgetSync ();

			if (!String.IsNullOrEmpty(syncUrl))
			{
				try 
				{
					var uri = new System.Uri(syncUrl);
					Pull = Database.CreatePullReplication(uri);
					Pull.Continuous = true;
					Pull.Changed += ReplicationChanged;

					Push = Database.CreatePushReplication(uri);
					Push.Continuous = true;
					Push.Changed += ReplicationChanged;

					Pull.Start();
					Push.Start();
				} 
				catch (Java.Lang.Throwable th)
				{
					Log.Debug(Tag, th, "UpdateSync Error");
				}
			}
		}

		private void ForgetSync()
		{
			if (Pull != null) {
				Pull.Changed -= ReplicationChanged;
				Pull.Stop();
				Pull = null;
			}

			if (Push != null) {
				Push.Changed -= ReplicationChanged;
				Push.Stop();
				Push = null;
			}
		}

		public void ReplicationChanged(object sender, ReplicationChangeEventArgs args)
		{
			Couchbase.Lite.Util.Log.D(Tag, "Replication Changed: {0}", args);

			var replicator = args.Source;

			var totalCount = replicator.ChangesCount;
			var completedCount = replicator.CompletedChangesCount;

			if (totalCount > 0 && completedCount < totalCount) {
				SetProgressBarIndeterminateVisibility(true);
			} else {
				SetProgressBarIndeterminateVisibility(false);
			}
		}

		private void SendSms(SMS sms)
		{
			//var doc = Database.GetDocument(Tag.ToString () + "_" + sms.Target);
			var doc = Database.CreateDocument();
			var props = new Dictionary<string, object>
			{
				{ "time", sms.Time},
				{ "type", sms.Type},
				{ "text", sms.Text}
			};

			doc.PutProperties(props);
					
		}
			
		public class ListLiveQueryAdapter : ConversationListViewAdapter 
		{
			public ListLiveQueryAdapter(Context context, LiveQuery query) 
				: base(context, query) { }

			public override Android.Views.View GetView(int position,
				Android.Views.View convertView, ViewGroup parent)
			{
				var view = convertView;
				if (view == null)
				{
					view = ((Activity)Context).LayoutInflater.Inflate(
						Resource.Layout.ConversationListItem, null);
				}

				var document = this[position];

				var text = view.FindViewById<TextView>(Resource.Id.text);
				text.Text = (string)document.GetProperty("text") + 
					System.Environment.NewLine + System.Environment.NewLine +
					DateTime.ParseExact((string)document.GetProperty("time"), "yyyyMMddHHmmssffff", null).TimeAgo();



				return view;

			}

		}
	}
}

