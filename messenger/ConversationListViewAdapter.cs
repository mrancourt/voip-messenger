using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Couchbase.Lite;
using Sharpen;
using System;
using System.Linq;
using JObject = Java.Lang.Object;
using CBDocument = Couchbase.Lite.Document;
using Java.Lang.Annotation;

namespace messenger
{
	public class ConversationListViewAdapter : BaseAdapter <CBDocument>
	{
		readonly LiveQuery query;

		private QueryEnumerator enumerator;

		protected Context Context;

		//public event EventHandler<QueryChangeEventArgs> DataSetChanged;

		public ConversationListViewAdapter (Context context, LiveQuery query)
		{
			this.Context = context;
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

