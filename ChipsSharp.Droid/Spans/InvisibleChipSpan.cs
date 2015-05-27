using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Style;
using ChipsSharp;
using Java.Lang;

namespace com.android.ex.chips.Spans
{
	public class InvisibleChipSpan : DrawableChipSpan
	{
		public InvisibleChipSpan(Drawable drawable, ChipEntry entry)
			: base(drawable, entry)
		{
		}

		public InvisibleChipSpan(Drawable drawable, ChipEntry entry, SpanAlign verticalAlignment)
			: base(drawable, entry, verticalAlignment)
		{
		}

		public InvisibleChipSpan(ChipEntry entry) : this(null, entry)
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
			return new Rect(0, 0, 0, 0);
		}

		public override void draw(Canvas canvas)
		{
			// Do nothing.
		}

		public override void Draw(Canvas canvas, ICharSequence text, int start, int end, float x, int top, int y, int bottom,
		                          Paint paint)
		{
			// Do nothing.
		}

		public override int GetSize(Paint paint, ICharSequence text, int start, int end, Paint.FontMetricsInt fm)
		{
			return 0;
		}
	}
}