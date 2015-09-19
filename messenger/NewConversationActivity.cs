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
using Utilities;

namespace messenger
{
	[Activity (Label = "@string/NewConversationActivity.label", Theme = "@style/Theme.Base")]			
	public class NewConversationActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			ActionBar.SetHomeButtonEnabled(true);
			ActionBar.SetDisplayHomeAsUpEnabled(true);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.NewConversation);

			var contactAdapter = new ContactAdapter (this);
			var contentListView = FindViewById<ListView> (Resource.Id.ContactsListView);
			contentListView.Adapter = contactAdapter;

			contentListView.ItemClick += (sender, e) => {
				// Get selected contact infos
				var contact = (Contact) contentListView.GetItemAtPosition(e.Position);

				var intent = new Intent(this, typeof(ConversationActivity));
				intent.PutExtra ("normalizedPhone", contact.NormalizedNumber);
				StartActivity(intent);
			};
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
	}
}