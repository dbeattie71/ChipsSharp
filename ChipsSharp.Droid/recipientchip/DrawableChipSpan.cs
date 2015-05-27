using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text.Style;
using Java.Lang;

namespace com.android.ex.chips.recipientchip
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
		public abstract String getDisplay();
		public abstract ChipEntry getEntry();
		public abstract void setOriginalText(String text);
		public abstract String getOriginalText();
		public abstract Rect getBounds();
		public abstract void draw(Canvas canvas);
	}
}