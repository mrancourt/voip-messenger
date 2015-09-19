using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Couchbase.Lite;
using Android.Util;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Views;
using Utilities;
using Gcm.Client;

namespace messenger
{
	[Activity (Label = "@string/MainActivity.label", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.Base")]
	public class MainActivity : Activity
	{

		static readonly string Tag = "conversation";
		Query Query { get; set; }
		Database Database { get; set; }
		LiveQuery LiveQuery { get; set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Register the device for Push Notifications
			GcmBroadcastRegister.Register (this);

			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			FloatingActionButton btnNewConversation = FindViewById<FloatingActionButton> (Resource.Id.btnNewConversation);
			ListView ConversationslistView = FindViewById<ListView>(Resource.Id.listViewConversations);

			btnNewConversation.Click += (sender, e) =>
			{
				var intent = new Intent(this, typeof(NewConversationActivity));
				StartActivity(intent);
			};

			ConversationslistView.ItemClick += (sender, e) => {
				// Get conversation document
				var doc = ConversationslistView.GetItemAtPosition(e.Position).Cast<Document>();
				var intent = new Intent(this, typeof(ConversationActivity));
				intent.PutExtra ("normalizedPhone", (string) doc.GetProperty("conversationId"));
				StartActivity(intent);
			};
			// Get database instance
			Database = Manager.SharedInstance.GetDatabase(Tag.ToLower());

			// Create a view and register its map function:
			var view = Database.GetView ("conversations");

			view.SetMap ((doc, emit) => {

				if (!doc.ContainsKey("type")) 
				{
					return;
				}

				// If document is a conversation, we emit it
				if (doc["type"].ToString() == Tag) {
					emit (
						doc["lastMessageTime"],
						doc
					);
				}

			}, "1");

			// Set up a query for a view that indexes conversations, to get every unique conversation:
			var query = Database.GetView("conversations").CreateQuery();
			query.Descending = true;
			query.Completed += (sender, e) => 
				Log.Verbose(Tag, e.ErrorInfo.ToString() ?? e.Rows.ToString());
			LiveQuery = query.ToLiveQuery ();

			// Bind listview adapyer to liveQuery
			ConversationslistView.Adapter = new ListLiveQueryAdapter(this, LiveQuery);
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
						Resource.Layout.ConversationsListItem, null);
				}

				var document = this[position];

				var contact = Contact.GetContactByPhone ((string)document.GetProperty ("conversationId"), Context);

				var txtContactName = view.FindViewById<TextView>(Resource.Id.txtContactName);
				var txtLastMessage = view.FindViewById<TextView>(Resource.Id.txtLastMessage);
				var txtLastMessageTime = view.FindViewById<TextView>(Resource.Id.txtLastMessageTime);
				var imgContactThumb = view.FindViewById<ImageView> (Resource.Id.imgContactThumb);
					
				txtContactName.Text = contact.DisplayName;

				if (document.GetProperty ("lastMessage") != null) {
					txtLastMessage.Text = (string)document.GetProperty ("lastMessage");
				}

				if (document.GetProperty ("lastMessageTime") != null) {
					txtLastMessageTime.Text = DateTime.ParseExact (
						(string)document.GetProperty ("lastMessageTime"), "yyyyMMddHHmmssffff", null
					).TimeAgo ();
				}

				// TODO : Add round image and placeholder image if contact does not have any 
				if (contact.PhotoThumbnailId != null && contact.GetThumbnailUri () != null) {
					imgContactThumb.SetImageURI (contact.GetThumbnailUri());
				}
					
				view.SetTag (Resource.String.NormalizedPhone, (string)document.GetProperty ("conversationId"));

				return view;

			}
		}
	}
}

