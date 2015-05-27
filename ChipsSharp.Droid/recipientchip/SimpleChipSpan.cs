using Android.Text;
using Java.Lang;

namespace com.android.ex.chips.recipientchip
{
	public class SimpleChipSpan : BaseChipSpan
	{
		private readonly String mDisplay;
		private readonly ChipEntry mEntry;
		private readonly String mValue;
		private String mOriginalText;
		private bool mSelected;

		public SimpleChipSpan(ChipEntry entry)
		{
			mDisplay = entry.getDisplayName();
			mValue = new String(entry.getDestination().Trim());
			mEntry = entry;
		}

		public override void setSelected(bool selected)
		{
			mSelected = selected;
		}

		public override bool isSelected()
		{
			return mSelected;
		}

		public override String getDisplay()
		{
			return mDisplay;
		}

		public override ChipEntry getEntry()
		{
			return mEntry;
		}

		public override void setOriginalText(String text)
		{
			if (TextUtils.IsEmpty(text))
			{
				mOriginalText = text;
			}
			else
			{
				mOriginalText = new String(text.Trim());
			}
		}

		public override String getOriginalText()
		{
			return !TextUtils.IsEmpty(mOriginalText) ? mOriginalText : mEntry.getDestination();
		}

		public string toString()
		{
			return mDisplay + " <" + mValue + ">";
		}
	}
}