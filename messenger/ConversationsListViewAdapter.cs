using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Couchbase.Lite;
using System;
using JObject = Java.Lang.Object;
using CBDocument = Couchbase.Lite.Document;
using Android.Util;

namespace messenger
{
	public class ConversationsListViewAdapter : BaseAdapter <CBDocument>
	{
		readonly LiveQuery query;

		QueryEnumerator enumerator;

		protected Context Context;

		public event EventHandler<QueryChangeEventArgs> DataSetChanged;

		public ConversationsListViewAdapter (Context context, LiveQuery query)
		{
			Context = context;
			this.query = query;
			query.Changed += (sender, e) => {
				enumerator = e.Rows;
				((Activity)context).RunOnUiThread(new Action (NotifyDataSetChanged));
			};

			//TODO: Revise
			query.Start();
		}

		public override int Count
		{
			get { 
				return enumerator != null ? enumerator.Count : 0; 
			}
		}

		public override CBDocument this[int index]
		{
			get {
				var val = enumerator != null ? enumerator.GetRow(index).Document : null;
				return val;
			}
		}

		public override long GetItemId(int i)
		{
			return enumerator.GetRow(i).SequenceNumber;
		}

		public override global::Android.Views.View GetView(int position, global::Android.Views.View convertView, ViewGroup parent)
		{
			return null;
		}

		public virtual void Invalidate()
		{
			if (query != null)
			{
				query.Stop();
			}
		}
	}
}

