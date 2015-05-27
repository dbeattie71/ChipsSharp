namespace ChipsSharp
{
	public class SimpleChipSpan : BaseChipSpan
	{
		private readonly string mDisplay;
		private readonly IChipEntry mEntry;
		private readonly string mValue;
		private string mOriginalText;
		private bool mSelected;

		public SimpleChipSpan(IChipEntry entry)
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

		public override IChipEntry getEntry()
		{
			return mEntry;
		}

		public override void setOriginalText(string text)
		{
			if (string.IsNullOrEmpty(text))
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
			if (!string.IsNullOrEmpty(mOriginalText))
				return mOriginalText;
			return mEntry.getDestination();
		}

		public string toString()
		{
			return mDisplay + " <" + mValue + ">";
		}
	}
}