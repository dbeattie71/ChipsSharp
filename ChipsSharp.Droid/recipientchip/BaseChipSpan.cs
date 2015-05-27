/**
 * BaseRecipientChip defines an object that contains information relevant to a
 * particular recipient.
 */

using Java.Lang;

namespace com.android.ex.chips.recipientchip
{
	public abstract class BaseChipSpan : Object, IBaseChipSpan
	{
		public abstract void setSelected(bool selected);
		public abstract bool isSelected();
		public abstract String getDisplay();
		public abstract ChipEntry getEntry();
		public abstract void setOriginalText(String text);
		public abstract String getOriginalText();
	}
}