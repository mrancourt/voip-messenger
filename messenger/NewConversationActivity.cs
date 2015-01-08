
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace messenger
{
	[Activity (Label = "@string/NewConversationActivity.label", Theme = "@style/Theme.Base")]			
	public class NewConversationActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.NewConversation);

			var contactAdapter = new ContactAdapter (this);
			var contentListView = FindViewById<ListView> (Resource.Id.ContactsListView);
			contentListView.Adapter = contactAdapter;
		
		}
	}
}

