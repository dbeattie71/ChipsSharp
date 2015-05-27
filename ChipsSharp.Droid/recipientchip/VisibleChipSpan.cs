using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Style;
using Java.Lang;

namespace com.android.ex.chips.recipientchip
{
	public class VisibleChipSpan : DrawableChipSpan
	{
		public VisibleChipSpan(Drawable drawable, ChipEntry entry)
			: base(drawable, entry)
		{
		}

		public VisibleChipSpan(Drawable drawable, ChipEntry entry, SpanAlign verticalAlignment)
			: base(drawable, entry, verticalAlignment)
		{
		}

		public override void setSelected(bool selected)
		{
			mDelegate.setSelected(selected);
		}

		public override bool isSelected()
		{
			return mDelegate.isSelected();
		}

		public override String getDisplay()
		{
			return mDelegate.getDisplay();
		}

		public override ChipEntry getEntry()
		{
			return mDelegate.getEntry();
		}

		public override void setOriginalText(String text)
		{
			mDelegate.setOriginalText(text);
		}

		public override String getOriginalText()
		{
			return mDelegate.getOriginalText();
		}

		public override Rect getBounds()
		{
			return Drawable.Bounds;
		}

		public override void draw(Canvas canvas)
		{
			Drawable.Draw(canvas);
		}

		public override string ToString()
		{
			return mDelegate.toString();
		}
	}
}