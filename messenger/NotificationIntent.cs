using System;
using Android.Content;
using Android.OS;

namespace messenger
{
	public static class NotificationIntent
	{
		public static Intent GetNewMessageIntent(Context context, Intent intent) {
			// Create the PendingIntent with the back stack
			// When the user clicks the notification, ConversationActivity will start up.
			var notificationIntent = new Intent(context, typeof(ConversationActivity));
			notificationIntent.PutExtras( intent.Extras); // Pass some values to SecondActivity.

			Android.App.TaskStackBuilder stackBuilder = Android.App.TaskStackBuilder.Create(context);
			stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(ConversationActivity)));
			stackBuilder.AddNextIntent(notificationIntent);

			return notificationIntent;
		}
	}
}

