using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android;
using Android.Media;

namespace messenger
{
	// Fragment that demonstrates options for displaying Heads-Up Notifications.
	public class HeadsUpNotificationFragment : Fragment
	{
		// NotificationId used for the notifications in this Fragment.
		public const int NOTIFICATION_ID = 1;

		private NotificationManager notificationManager;

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			notificationManager = (NotificationManager)Activity.GetSystemService (Context.NotificationService);
		}

		public static HeadsUpNotificationFragment NewInstance()
		{
			var fragment = new HeadsUpNotificationFragment ();
			fragment.RetainInstance = true;
			return fragment;
		}
			
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate (Resource.Layout.fragment_heads_up_notification, container, false);
		}
			

		// Creates a new notification depending on makeHeadsUpNotification.
		public Notification CreateNotification(Context context, Intent intent)
		{
		
			// indicate that this sms comes from server
			intent.PutExtra ("source", "server");
		
			// Parse bundle extras
			string contactId = intent.GetStringExtra("normalizedPhone");
			string message = intent.GetStringExtra("message");

			// Getting contact infos
			var contact = Contact.GetContactByPhone (contactId, context);

			var builder = new Notification.Builder (context)
				.SetSmallIcon (Resource.Drawable.icon)
				.SetPriority ((int)NotificationPriority.Default)
				.SetCategory (Notification.CategoryMessage)
				.SetContentTitle (contact.DisplayName != "" ? contact.DisplayName : contactId)
				.SetContentText (message)
				.SetSound (RingtoneManager.GetDefaultUri (RingtoneType.Notification));
				
			var fullScreenPendingIntent = PendingIntent.GetActivity (context, 0,
				intent, PendingIntentFlags.CancelCurrent);
				builder.SetContentText (message)
					   .SetFullScreenIntent (fullScreenPendingIntent, true)
					   .SetContentIntent (fullScreenPendingIntent);

			return builder.Build ();
		}
	}
}
