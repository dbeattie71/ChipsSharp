using Android.Content.Res;
using Android.Net;
using Android.Provider;

namespace com.dbeattie
{
	public class Queries
	{
		public static PhoneQuery PHONE = new PhoneQuery(new[]
		{
			ContactsContract.Contacts.InterfaceConsts.DisplayName,
			ContactsContract.CommonDataKinds.Phone.Number,
			ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type,
			ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label,
			ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
			ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Id,
			ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri,
			ContactsContract.Contacts.InterfaceConsts.DisplayNameSource,
			ContactsContract.Contacts.InterfaceConsts.LookupKey,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Mimetype
		},
		                                                ContactsContract.CommonDataKinds.Phone.ContentFilterUri,
		                                                ContactsContract.CommonDataKinds.Phone.ContentUri);

		public static EmailQuery EMAIL = new EmailQuery(new[]
		{
			ContactsContract.Contacts.InterfaceConsts.DisplayName,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Label,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Id,
			ContactsContract.Contacts.InterfaceConsts.PhotoThumbnailUri,
			ContactsContract.Contacts.InterfaceConsts.DisplayNameSource,
			ContactsContract.Contacts.InterfaceConsts.LookupKey,
			ContactsContract.CommonDataKinds.Email.InterfaceConsts.Mimetype
		},
		                                                ContactsContract.CommonDataKinds.Email.ContentFilterUri,
		                                                ContactsContract.CommonDataKinds.Email.ContentUri);

		public class PhoneQuery : Query
		{
			public PhoneQuery(string[] projection, Uri contentFilter, Uri content)
				: base(projection, contentFilter, content)
			{
			}

			public override string getTypeLabel(Resources res, int type, string label)
			{
				return ContactsContract.CommonDataKinds.Phone.GetTypeLabel(res, (PhoneDataKind) type, label);
			}
		}

		public class EmailQuery : Query
		{
			public EmailQuery(string[] projection, Uri contentFilter, Uri content)
				: base(projection, contentFilter, content)
			{
			}

			public override string getTypeLabel(Resources res, int type, string label)
			{
				return ContactsContract.CommonDataKinds.Email.GetTypeLabel(res, (EmailDataKind) type, label);
			}
		}

		public abstract class Query
		{
			public static int NAME = 0; // String
			public static int DESTINATION = 1; // String
			public static int DESTINATION_TYPE = 2; // int
			public static int DESTINATION_LABEL = 3; // String
			public static int CONTACT_ID = 4; // long
			public static int DATA_ID = 5; // long
			public static int PHOTO_THUMBNAIL_URI = 6; // String
			public static int DISPLAY_NAME_SOURCE = 7; // int
			public static int LOOKUP_KEY = 8; // String
			public static int MIME_TYPE = 9; // String
			private readonly Uri mContentFilterUri;
			private readonly Uri mContentUri;
			private readonly string[] mProjection;

			protected Query(string[] projection, Uri contentFilter, Uri content)
			{
				mProjection = projection;
				mContentFilterUri = contentFilter;
				mContentUri = content;
			}

			public string[] getProjection()
			{
				return mProjection;
			}

			public Uri getContentFilterUri()
			{
				return mContentFilterUri;
			}

			public Uri getContentUri()
			{
				return mContentUri;
			}

			public abstract string getTypeLabel(Resources res, int type, string label);
		}
	}
}