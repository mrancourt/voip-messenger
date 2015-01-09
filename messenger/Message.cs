using System.Collections.Generic;
using Couchbase.Lite;
using Couchbase.Lite.Util;
using Sharpen;
using System.Globalization;
using System;
using System.Reflection;

namespace messenger
{
	public class Message
	{
		// TODO : A verifer/Changer
		private const string DocType = "list";
		private const string ViewName = "lists";

		public Message ()
		{
		}

		public static Query GetQuery(Database database)
		{
			var view = database.GetView(ViewName);
			if (view.Map == null)
			{
				view.SetMap((document, emitter) => 
					{
						object type;
						document.TryGetValue("type", out type);

						if (Message.DocType.Equals ((string)type)) {
							emitter (document["text"], document);
						}
					}, "1");
			}
			var query = view.CreateQuery();
			return query;
		}
	}
}

