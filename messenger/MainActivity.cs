using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace messenger
{
	[Activity (Label = "messenger", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button btnNewConversation = FindViewById<Button> (Resource.Id.btnNewConversation);
			Button btnExistingConversation = FindViewById<Button> (Resource.Id.btnExistingConversation);

			btnNewConversation.Click += (sender, e) => {
				var intent = new Intent(this, typeof(NewConversationActivity));
				StartActivity(intent);
			};

			btnExistingConversation.Click += (sender, e) => {
				var intent = new Intent(this, typeof(ConversationActivity));
				StartActivity(intent);
			};
		}

	}
}


