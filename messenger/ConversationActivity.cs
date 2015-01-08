
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

using Android.Database;
using Android.Provider;

namespace messenger
{
	[Activity (Label = "@string/ConversationActivity.label", Theme = "@style/Theme.Base")]			
	public class ConversationActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Activate Back button in Action Bar
			ActionBar.SetHomeButtonEnabled(true);
			ActionBar.SetDisplayHomeAsUpEnabled(true);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Conversation);

			// Load contact infos
			long contactId = Intent.GetLongExtra("Id", -1);
			Contact contact = new Contact ().GetContactById(contactId, this);

			// Set ActionBar to contact name
			this.Title = contact.DisplayName;

			TextView txtContactInfo = FindViewById<TextView> (Resource.Id.txtContactInfo);
			txtContactInfo.Text = contact.ToString();
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
	}
}

