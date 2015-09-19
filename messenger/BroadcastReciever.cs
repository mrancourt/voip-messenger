using System;
using Android.Content;
using Android.App;

namespace messenger
{
	[BroadcastReceiver(Permission= "com.google.android.c2dm.permission.SEND")]
	[IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new string[] {"@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { "com.google.android.c2dm.intent.REGISTRATION" }, Categories = new string[] {"@PACKAGE_NAME@" })]
	[IntentFilter(new string[] { "com.google.android.gcm.intent.RETRY" }, Categories = new string[] { "@PACKAGE_NAME@"})]
	public class MyGCMBroadcastReceiver : BroadcastReceiver
	{
		const string TAG = "PushHandlerBroadcastReceiver";
		public override void OnReceive(Context context, Intent intent)
		{
			MyIntentService.RunIntentInService(context, intent);
			SetResult(Result.Ok, null, null);
		}

	}
}

