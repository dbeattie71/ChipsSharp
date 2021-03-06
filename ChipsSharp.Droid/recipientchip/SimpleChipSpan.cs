using Android.Text;
using ChipsSharp;

namespace com.android.ex.chips.recipientchip
{
	public class SimpleChipSpan : BaseChipSpan
	{
		private readonly string mDisplay;
		private readonly ChipEntry mEntry;
		private readonly string mValue;
		private string mOriginalText;
		private bool mSelected;

		public SimpleChipSpan(ChipEntry entry)
		{
			mDisplay = entry.getDisplayName();
			mValue = entry.getDestination().Trim();
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

		public override string getDisplay()
		{
			return mDisplay;
		}

		public override ChipEntry getEntry()
		{
			return mEntry;
		}

		public override void setOriginalText(string text)
		{
			if (TextUtils.IsEmpty(text))
			{
				mOriginalText = text;
			}
			else
			{
				mOriginalText = text.Trim();
			}
		}

		public override string getOriginalText()
		{
			if (!TextUtils.IsEmpty(mOriginalText)) return mOriginalText;
			return mEntry.getDestination();
		}

		public string toString()
		{
			return mDisplay + " <" + mValue + ">";
		}
	}
}