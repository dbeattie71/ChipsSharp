using System;

namespace ChipsSharp
{
	public class ChipEntry
	{
		private readonly String mDestination;
		private readonly String mDisplayName;
		private readonly bool mIsDivider;
		private readonly Uri mPhotoThumbnailUri;
		private byte[] mPhotoBytes;

		public ChipEntry(
			String displayName,
			String destination,
			Uri photoThumbnailUri)
		{
			mDisplayName = displayName;
			mDestination = destination;
			mPhotoThumbnailUri = photoThumbnailUri;
			mPhotoBytes = null;
			mIsDivider = false;
		}

		public String getDisplayName()
		{
			return mDisplayName;
		}

		public String getDestination()
		{
			return mDestination;
		}

		public Uri getPhotoThumbnailUri()
		{
			return mPhotoThumbnailUri;
		}

		/** This can be called outside main Looper thread. */
		//public synchronized void setPhotoBytes(byte[] photoBytes) {
		public void setPhotoBytes(byte[] photoBytes)
		{
			mPhotoBytes = photoBytes;
		}

		/** This can be called outside main Looper thread. */
		//public synchronized byte[] getPhotoBytes() {
		public byte[] getPhotoBytes()
		{
			return mPhotoBytes;
		}

		public bool isSeparator()
		{
			return mIsDivider;
		}

		public override string ToString()
		{
			return mDisplayName + " <" + mDestination + ">";
		}
	}
}