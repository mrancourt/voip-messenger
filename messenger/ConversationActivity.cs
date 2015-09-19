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
using Utilities;
using Newtonsoft.Json;
using System.Threading;

namespace messenger
{
	[Activity (WindowSoftInputMode = SoftInput.AdjustResize, Label = "@string/ConversationActivity.label", Theme = "@style/Theme.Base")]			
	public class ConversationActivity : Activity
	{

		static readonly string Tag = "conversation";

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
			string normalizedPhone = Intent.GetStringExtra("normalizedPhone");
			string message = Intent.GetStringExtra("message");

			Contact contact = Contact.GetContactByPhone(normalizedPhone, this);

			// Get database instance
			Database = Manager.SharedInstance.GetDatabase(Tag.ToLower());

			var smsManager = new SMSManager (Database);
			var sms = new SMS(normalizedPhone, message); 

			// Create conversation document if not exist
			initConversation (sms);

			// If message comes from notification, add it to db
			string source = (string) Intent.GetStringExtra ("source");
			if (source == "server") {
				sms.Source = source;
				smsManager.AddMessage (sms);
			}

			// Get previous messages
			// TODO : Filer messages to get only those from current conversation
			Query = smsManager.GetQuery(sms.Target);
			Query.Completed += (sender, e) => 
				Log.Verbose(Tag, e.ErrorInfo.ToString() ?? e.Rows.ToString());
			LiveQuery = Query.ToLiveQuery();
		
			SetContentView (Resource.Layout.Conversation);

			// Activate Back button in Action Bar
			ActionBar.SetHomeButtonEnabled(true);
			ActionBar.SetDisplayHomeAsUpEnabled(true);

			// Rethrive layout fields
			TextView newMessageText = FindViewById<TextView> (Resource.Id.txtMessageBox);
			FloatingActionButton sendMessageButton = FindViewById<FloatingActionButton> (Resource.Id.btnSendMessage);
			LinearLayout layout = FindViewById<LinearLayout> (Resource.Id.mainLinearLayout);
			ListView listView = FindViewById<ListView>(Resource.Id.listViewMessages);

			// Set ActionBar to contact name
			this.Title = contact.DisplayName + " " + contact.NormalizedNumber;

			sendMessageButton.Click += (sender, e) => {
				sms.Message = newMessageText.Text;
				sms.Source = "app";
				Background(sms);

				newMessageText.Text = "";
			};

			Database.Changed += (sender, e) => {
				var changes = e.Changes.ToList();
				foreach (DocumentChange change in changes) {
					Log.Verbose(Tag, "Document " + change.DocumentId + " changed");
				}
			};
				
			// Bind listview adapyer to liveQuery
			listView.Adapter = new ListLiveQueryAdapter(this, LiveQuery);
		}

		void Background(SMS sms) {

			// Starting a new thread to do the workload of sending the sms 
			new Thread(new ThreadStart(() =>
				{

					// Send sms
					VoipMsResponse response = SMSManager.SendSMS(sms);

					// An error ocured while sending sms... Show error message
					/*if (response.error != null) {
						RunOnUiThread ( () => {
							//response.error txtMessageStatus.Text;
						});
					}*/

					// Save message to database, add it to the listview
					RunOnUiThread ( () => {
						var smsManager = new SMSManager (Database);
						smsManager.AddMessage(sms);
					});
						
				}
			)).Start();
		}

		/**
		 * Handle back button
		 **/
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

		// Create document for conversation if not exists
		void initConversation (SMS sms) {

			// Try to get document 
			var doc = Database.GetExistingDocument (sms.Target);

			// Document does not exists, let's create a new one
			if (doc == null) {
				var properties = new Dictionary<string, object>
				{
					{ "conversationId", sms.Target },
					{ "type", Tag.ToString() },
					{ "lastMessage", null },
					{ "lastMessageTime", null },
				};
				var document = Database.GetDocument(sms.Target);
				var rev = document.PutProperties(properties);
			}
		}

		void UpdateSync()
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

		void ForgetSync()
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
			
		public class ListLiveQueryAdapter : ConversationListViewAdapter 
		{
			public ListLiveQueryAdapter(Context context, LiveQuery query) 
				: base(context, query) { }

			public override Android.Views.View GetView(int position,
				Android.Views.View convertView, ViewGroup parent)
			{
				var document = this [position];

				var view = convertView;
				if (view == null)
				{

					int messageLayout;

					Console.WriteLine ((string)document.GetProperty ("source"));

					if ("app" == (string)document.GetProperty ("source")) {
						messageLayout = Resource.Layout.ConversationListItemOut;
					} else {
						messageLayout = Resource.Layout.ConversationListItemIn;
					}

					view = ((Activity)Context).LayoutInflater.Inflate(messageLayout, null);
				}


				var text = view.FindViewById<TextView>(Resource.Id.text);
				text.Text = (string)document.GetProperty("text") + 
					System.Environment.NewLine + System.Environment.NewLine +
					DateTime.ParseExact((string)document.GetProperty("time"), "yyyyMMddHHmmssffff", null).TimeAgo();
					
				return view;

			}
		}
	}
}