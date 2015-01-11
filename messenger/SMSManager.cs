using System;
using System.Reflection;
using Couchbase.Lite;
using Android.Util;

namespace messenger
{
	public static class SMSManager
	{
		// TODO : A verifer/Changer
		readonly static string DocType = typeof(SMS).Name.ToLower();
		const string ViewName = "messages";

		public static Query GetQuery(Database database, string normalizedPhone)
		{
			var view = database.GetView(ViewName + "_" + normalizedPhone);

			if (view.Map == null)
			{
				view.SetMap ((doc, emit) => {

					if (!doc.ContainsKey("type")) 
					{
						return;
					}

					// If document is a conversation, we emit it
					if (doc["type"].ToString() == DocType && 
						doc["conversationId"].ToString() == normalizedPhone) {
						emit (
							doc,
							doc["conversationId"]);
					}

				}, "6");
			}
			var query = view.CreateQuery();
			return query;
		}
	}
}

