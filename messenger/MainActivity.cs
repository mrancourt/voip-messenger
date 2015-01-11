using System;

using Android.App;
using Android.Content;
//using Android.Runtime;
//using Android.Views;
using Android.Widget;
using Android.OS;
using Couchbase.Lite;
using Android.Util;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Views;

namespace messenger
{
	[Activity (Label = "@string/MainActivity.label", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.Base")]
	public class MainActivity : Activity
	{

		static readonly string Tag = "conversation";
		Query Query { get; set; }
		Database Database { get; set; }

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button btnExistingConversation = FindViewById<Button> (Resource.Id.btnExistingConversation);
			FloatingActionButton btnNewConversation = FindViewById<FloatingActionButton> (Resource.Id.btnNewConversation);
			TextView txtConversationList = FindViewById<TextView> (Resource.Id.txtConversationList);

			btnNewConversation.Click += (sender, e) =>
			{
				var intent = new Intent(this, typeof(NewConversationActivity));
				StartActivity(intent);
			};

			btnExistingConversation.Click += (sender, e) => {
				var intent = new Intent(this, typeof(ConversationActivity));
				StartActivity(intent);
			};

			// Get database instance
			Database = Manager.SharedInstance.GetDatabase(Tag.ToLower());

			// Create a view and register its map function:
			var view = Database.GetView ("convos");

			view.SetMap ((doc, emit) => {
			
				if (!doc.ContainsKey("type")) 
				{
					return;
				}

				// If document is a conversation, we emit it
				if (doc["type"].ToString() == Tag) {
					emit (
						doc,
						doc["conversationId"]);
				}
					
			}, "1");


			// Set up a query for a view that indexes blog posts, to get the latest:
			var query = Database.GetView("convos").CreateQuery();
			query.Descending = true;
			query.Limit = 20;


			var rows = query.Run();

			foreach (var row in rows) 
			{

				IDictionary<string, object> props = row.Document.Properties;

				txtConversationList.Text += System.Environment.NewLine;

				foreach (var prop in props) {
					txtConversationList.Text += System.Environment.NewLine +
						prop.Key + " => " + prop;
				}
			}

			// ####################################################################
			// Intent convoIntent = new Intent(this, typeof(ConversationActivity));
			// convoIntent.PutExtra ("Id", (long)165);
			// StartActivity (convoIntent);
			// ####################################################################

		}
	}
}


