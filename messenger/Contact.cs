using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Provider;
using System.Collections.Generic;

namespace messenger
{
	public class Contact
	{
		public long Id { get; set; }
		public string DisplayName { get; set; }
		public string PhotoThumbnailId { get; set; }
		public string PhoneNumber { get; set; }

		public Contact GetContactById(long Id, Activity activity) {
			Contact contact = new Contact() ;

			var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

			string[] projection = {
				ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
				ContactsContract.Contacts.InterfaceConsts.DisplayName,
				ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri,
				ContactsContract.CommonDataKinds.Phone.Number
			};

			// Build query statement
			string selection = ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + "= ?";
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
					PhoneNumber = cursor.GetString (cursor.GetColumnIndex (projection [3])),
				};
			} 

			return contact;
		}

		public override string ToString() {
			string contactInfo = 
				Id + Environment.NewLine + 
				DisplayName + Environment.NewLine + 
				PhoneNumber + Environment.NewLine + 
				PhotoThumbnailId ;

			return contactInfo;
		}

	}
}