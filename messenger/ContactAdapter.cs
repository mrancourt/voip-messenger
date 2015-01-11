using System;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Database;
using Android.Content;
using Android.Provider;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Animation;

// Refactoriser permettre de passer des parametres a la fonction fillcontacts. Ex pour recuperer un seul contact.

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
				ContactsContract.CommonDataKinds.Phone.Number,
				ContactsContract.CommonDataKinds.Phone.NormalizedNumber
			};

			// Build query statement
			string selection = ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber + "= ?";
			string[] selectionArgs = { "1" };
			//string order = ContactsContract.Contacts.InterfaceConsts.Starred + " DESC";
			string order = ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + " DESC";


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
						Number = cursor.GetString(cursor.GetColumnIndex(projection[3])),
						NormalizedNumber = cursor.GetString(cursor.GetColumnIndex(projection[4]))
					});

				} while (cursor.MoveToNext ());
			}
		}

		public override int Count {
			get { return _contactList.Count; }
		}

		public override Java.Lang.Object GetItem (int position) {
			// wrap Contact in a Java.Lang.Object
			return _contactList [position];
		}

		public override long GetItemId (int position) {
			return _contactList [position].Id;
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView ?? _activity.LayoutInflater.Inflate (
				Resource.Layout.ContactListItem, parent, false);
			var contactName = view.FindViewById<TextView> (Resource.Id.ContactName);
			var lastMessage = view.FindViewById<TextView> (Resource.Id.LastMessage);
			var contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);
			contactName.Text = _contactList [position].DisplayName;
			lastMessage.Text = "Last Message...";

			if (_contactList [position].PhotoThumbnailId == null) {
				contactImage = view.FindViewById<ImageView> (Resource.Id.ContactImage);

				// TODO: corriger ca
				Bitmap contactImageBmp = BitmapFactory.DecodeResource (parent.Context.Resources, 1);

				if (contactImageBmp == null) {

					Bitmap.Config conf = Bitmap.Config.Argb8888; // see other conf types
					Bitmap bmp = Bitmap.CreateBitmap(200, 200, conf);
					bmp.EraseColor(Android.Graphics.Color.ParseColor("#0099CC"));

					Drawable circleContactImage = new CircleDrawable(bmp);
					contactImage.SetImageDrawable (circleContactImage);
				}

			}  
			else 
			{
				var contactUri = ContentUris.WithAppendedId (
					ContactsContract.Contacts.ContentUri, _contactList [position].Id);
				var contactPhotoUri = Android.Net.Uri.WithAppendedPath (contactUri,
					Contacts.Photos.ContentDirectory);
				contactImage.SetImageURI (contactPhotoUri);
			}

			view.SetTag (Resource.String.NormalizedPhone, _contactList [position].DisplayName);

			return view;
		}
	}
}

