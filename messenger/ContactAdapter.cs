//using System;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Provider;
using Android.Database;


// Refactoriser permettre de passer des parametres a la fonction fillcontacts. Ex pour recuperer un seul contact.
using Android.Net;
using System;

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

			var txtContactName = view.FindViewById<TextView> (Resource.Id.txtContactName);
			var txtPhoneNumber = view.FindViewById<TextView> (Resource.Id.txtPhoneNumber);
			var imgContactThumb = view.FindViewById<ImageView> (Resource.Id.imgContactThumb);
			txtPhoneNumber.Text = _contactList [position].Number;

			// For each number but the first one
			if (position > 0) {
				// We are on the first contact's number
				if (_contactList [position - 1].Id != _contactList [position].Id) {

					// Set contact name
					txtContactName.Text = _contactList [position].DisplayName;

					// Handle contact's image
					if (_contactList [position].PhotoThumbnailId == null) {
						imgContactThumb = view.FindViewById<ImageView> (Resource.Id.imgContactThumb);

						// TODO: corriger ca
						Bitmap contactImageBmp = BitmapFactory.DecodeResource (parent.Context.Resources, 1);

						if (contactImageBmp == null) {

							Bitmap.Config conf = Bitmap.Config.Argb8888; // see other conf types
							Bitmap bmp = Bitmap.CreateBitmap (200, 200, conf);
							bmp.EraseColor (Android.Graphics.Color.ParseColor ("#0099CC"));

							Drawable circleContactImage = new CircleDrawable (bmp);
							imgContactThumb.SetImageDrawable (circleContactImage);
						}

					} else {
						imgContactThumb.SetImageURI ( _contactList[position].GetThumbnailUri());
					}
				} 
				// We are on additional contact's number
				else 
				{
					// Remove image
					imgContactThumb.SetImageDrawable(null);

					// Empty contact name
					txtContactName.Text = null;
				}
			}


			view.SetTag (Resource.String.NormalizedPhone, _contactList [position].DisplayName);

			return view;
		}
	}
}

