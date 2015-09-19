using System;
using Android.App;
using Android.Content;
using Gcm.Client;

namespace messenger
{
	[BroadcastReceiver(Permission=Constants.PERMISSION_GCM_INTENTS)]
	[IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE }, 
		Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK }, 
		Categories = new string[] { "@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY }, 
		Categories = new string[] { "@PACKAGE_NAME@" })]
	public class GcmBroadcastReceiver : GcmBroadcastReceiverBase<GcmService>
	{
		//IMPORTANT: Change this to your own Sender ID!
		//The SENDER_ID is your Google API Console App Project Number
		public static string[] SENDER_IDS = new string[] {"200191496227"};
	}

	public static class GcmBroadcastRegister {
		public static void Register(Context context) {
			//Check to see that GCM is supported and that the manifest has the correct information
			GcmClient.CheckDevice(context);
			GcmClient.CheckManifest(context);

			//Call to Register the device for Push Notifications
			GcmClient.Register(context, GcmBroadcastReceiver.SENDER_IDS);
		}
	}

	[Service] //Must use the service tag
	public class GcmService : GcmServiceBase
	{
		public GcmService() : base(GcmBroadcastReceiver.SENDER_IDS) { }

		protected override void OnRegistered (Context context, string registrationId)
		{
			//Receive registration Id for sending GCM Push Notifications to
			Console.WriteLine(registrationId);
		}

		protected override void OnUnRegistered (Context context, string registrationId)
		{
			//Receive notice that the app no longer wants notifications
			Console.WriteLine("//Receive notice that the app no longer wants notifications");

		}

		protected override void OnMessage (Context context, Intent intent)
		{
			const int priority = 10;

			// Finally publish the notification
			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			var notificationIntent = NotificationIntent.GetNewMessageIntent (context, intent);
			var notification = HeadsUpNotificationFragment.NewInstance ().CreateNotification (context, notificationIntent);

			// Create and send notification
			notificationManager.Notify(priority, notification);

		}

		protected override bool OnRecoverableError (Context context, string errorId)
		{
			//Some recoverable error happened
			Console.WriteLine("//Some recoverable error happened");

			return true;

		}

		protected override void OnError (Context context, string errorId)
		{
			//Some more serious error happened
			Console.WriteLine("//Some more serious error happened");

		}
	}
}

