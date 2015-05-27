using Android.Graphics;

namespace com.android.ex.chips.recipientchip
{
	public interface IDrawableChipSpan : IBaseChipSpan
	{
		Rect getBounds();
		void draw(Canvas canvas);
	}
}