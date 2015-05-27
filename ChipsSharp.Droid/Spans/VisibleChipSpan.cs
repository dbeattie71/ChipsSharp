using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Style;
using ChipsSharp;

namespace com.android.ex.chips.Spans
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

		public override string getDisplay()
		{
			return mDelegate.getDisplay();
		}

		public override ChipEntry getEntry()
		{
			return mDelegate.getEntry();
		}

		public override void setOriginalText(string text)
		{
			mDelegate.setOriginalText(text);
		}

		public override string getOriginalText()
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