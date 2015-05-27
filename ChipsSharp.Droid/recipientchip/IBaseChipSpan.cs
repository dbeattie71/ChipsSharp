using Java.Lang;

namespace com.android.ex.chips.recipientchip
{
	public interface IBaseChipSpan
	{
		void setSelected(bool selected);
		bool isSelected();
		String getDisplay();
		ChipEntry getEntry();
		void setOriginalText(String text);
		String getOriginalText();
	}
}