using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Style;
using ChipsSharp;

namespace com.android.ex.chips.Spans
{
	public abstract class DrawableChipSpan : ImageSpan, IDrawableChipSpan
	{
		protected readonly SimpleChipSpan mDelegate;

		protected DrawableChipSpan(Drawable drawable, ChipEntry entry)
			: this(drawable, entry, SpanAlign.Bottom)
		{
		}

		protected DrawableChipSpan(Drawable drawable, ChipEntry entry, SpanAlign verticalAlignment)
			: base(drawable, verticalAlignment)
		{
			mDelegate = new SimpleChipSpan(entry);
		}

		public abstract void setSelected(bool selected);
		public abstract bool isSelected();
		public abstract string getDisplay();
		public abstract ChipEntry getEntry();
		public abstract void setOriginalText(string text);
		public abstract string getOriginalText();
		public abstract Rect getBounds();
		public abstract void draw(Canvas canvas);
	}
}