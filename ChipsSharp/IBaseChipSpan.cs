namespace ChipsSharp
{
	public interface IBaseChipSpan
	{
		void setSelected(bool selected);
		bool isSelected();
		string getDisplay();
		IChipEntry getEntry();
		void setOriginalText(string text);
		string getOriginalText();
	}
}