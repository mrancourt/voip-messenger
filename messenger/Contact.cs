using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;
using System.Collections.Generic;
using Android.Net;

namespace messenger
{
	public class Contact : Java.Lang.Object
	{
		public long Id { get; set; }
		public string DisplayName { get; set; }
		public string PhotoThumbnailId { get; set; }
		public string Number { get; set; }
		public string NormalizedNumber { get; set; }

		public static Contact GetContactById(long Id, Activity activity) {
			// Build query statement
			const string selection = ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + "= ?";
			string[] selectionArgs = { Id.ToString() };

			Contact contact = GetContact (activity, selection, selectionArgs);

			return contact ;
		}

		public static Contact GetContactByPhone(string normalizedPhone, Activity activity) {
			// Build query statement
			const string selection = ContactsContract.CommonDataKinds.Phone.NormalizedNumber + "= ?";
			string[] selectionArgs = { normalizedPhone.ToString() };

			Contact contact = Contact.GetContact (activity, selection, selectionArgs);

			return contact ;
		}

		protected static Contact GetContact (Activity activity, string selection = null, string[] selectionArgs = null) 
		{
			var contact = new Contact() ;

			var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

			string[] projection = {
				ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
				ContactsContract.Contacts.InterfaceConsts.DisplayName,
				ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri,
				ContactsContract.CommonDataKinds.Phone.Number,
				ContactsContract.CommonDataKinds.Phone.NormalizedNumber

			};
				
			// Load query results
			var loader = new CursorLoader (activity, uri, projection, selection, selectionArgs, null);
			var cursor = (ICursor)loader.LoadInBackground ();

			if (cursor.MoveToFirst ()) {
				contact = new Contact {
					Id = cursor.GetLong (cursor.GetColumnIndex (projection [0])),
					DisplayName = cursor.GetString (cursor.GetColumnIndex (projection [1])),
					PhotoThumbnailId = cursor.GetString (cursor.GetColumnIndex (projection [2])),
					Number = cursor.GetString (cursor.GetColumnIndex (projection [3])),
					NormalizedNumber = cursor.GetString (cursor.GetColumnIndex (projection [4]))
				};
			} 

			return contact;
		}
	}
}