namespace ChipsSharp
{
	public abstract class BaseChipSpan : IBaseChipSpan
	{
		public abstract void setSelected(bool selected);
		public abstract bool isSelected();
		public abstract string getDisplay();
		public abstract ChipEntry getEntry();
		public abstract void setOriginalText(string text);
		public abstract string getOriginalText();
	}
}