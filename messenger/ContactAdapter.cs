using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Database;
using Android.Content;
using Android.Provider;
using System.Collections.Generic;

namespace messenger
{
	public class ContactAdapter : BaseAdapter
	{

		List<Contact> _contactList;
		Activity _activity;

		public ContactAdapter (Activity activity)
		{
			_activity = activity;
			FillContacts ();
		}

		void FillContacts () 
		{
			//var uri = ContactsContract.Contacts.ContentUri;
			var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;

			string[] projection = {
				ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
				ContactsContract.Contacts.InterfaceConsts.DisplayName,
				ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri,
				ContactsContract.CommonDataKinds.Phone.Number
			};

			// Build query statement
			string selection = ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber + "= ?";
			string[] selectionArgs = { "1" };
			string order = ContactsContract.Contacts.InterfaceConsts.Starred + " DESC";

			// Load query results
			var loader = new CursorLoader (_activity, uri, projection, selection, selectionArgs, order);
			var cursor = (ICursor)loader.LoadInBackground ();

			_contactList = new List<Contact> ();

			if (cursor.MoveToFirst ()) 
			{
				do {
					_contactList.Add(new Contact{
						Id = cursor.GetLong(cursor.GetColumnIndex(projection[0])),
						DisplayName = cursor.GetString(cursor.GetColumnIndex(projection[1])),
						PhotoThumbnailId = cursor.GetString(cursor.GetColumnIndex(projection[2])),
						PhoneNumber = cursor.GetString(cursor.GetColumnIndex(projection[3]))
					});
				} while (cursor.MoveToNext ());
			}
		}

		public override int Count {
			get { return _contactList.Count; }
		}

		public override Java.Lang.Object GetItem (int position) {
			// could wrap a Contact in a Java.Lang.Object
			// to return it here if needed
			return null;
		}

		public override long GetItemId (int position) {
			return _contactList [position].Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? _activity.LayoutInflater.Inflate (
				Resource.Layout.ContactListItem, parent, false);
			var contactName = view.FindViewById<TextView> (Resource.Id.ContactName);
			var contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			contactName.Text = _contactList [position].DisplayName;

			contactName.Text = _contactList [position].ToString();

			if (_contactList [position].PhotoThumbnailId == null) {
				contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
				contactImage.SetImageResource (Resource.Drawable.contactImage);
			}  else {
				var contactUri = ContentUris.WithAppendedId (
					ContactsContract.Contacts.ContentUri, _contactList [position].Id);
				var contactPhotoUri = Android.Net.Uri.WithAppendedPath (contactUri,
					Contacts.Photos.ContentDirectory);
				contactImage.SetImageURI (contactPhotoUri);
			}
			return view;
		}
	}
}

