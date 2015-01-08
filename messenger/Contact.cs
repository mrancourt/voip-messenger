using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using Android.App;
using Android.Content;
//using Android.OS;
//using Android.Views;
//using Android.Widget;

using Android.Database;
using Android.Provider;

namespace messenger
{
	public class Contact
	{
		public long Id { get; set; }
		public string DisplayName { get; set; }
		public string PhotoThumbnailId { get; set; }

		public Contact ()
		{

		}

		public Contact GetContactById(long Id, Activity activity) {
			Contact contact = new Contact() ;

			var uri = ContactsContract.Contacts.ContentUri;

			string[] projection = {
				ContactsContract.Contacts.InterfaceConsts.Id,
				ContactsContract.Contacts.InterfaceConsts.DisplayName,
				ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri
			};

			// Build query statement
			string selection = ContactsContract.Contacts.InterfaceConsts.Id + "= ?";
			string[] selectionArgs = { Id.ToString() };

			// Load query results
			var loader = new CursorLoader (activity, uri, projection, selection, selectionArgs, null);
			var cursor = (ICursor)loader.LoadInBackground ();

			List<Contact> contactList = new List<Contact> ();

			if (cursor.MoveToFirst ()) {
				contact = new Contact {
					Id = cursor.GetLong (cursor.GetColumnIndex (projection [0])),
					DisplayName = cursor.GetString (cursor.GetColumnIndex (projection [1])),
					PhotoThumbnailId = cursor.GetString (cursor.GetColumnIndex (projection [2])),
				};
			} 

			return contact;
		}

	}
}