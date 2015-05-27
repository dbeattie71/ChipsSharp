using Android.Graphics;
using ChipsSharp;

namespace com.android.ex.chips
{
	public interface IDrawableChipSpan : IBaseChipSpan
	{
		Rect getBounds();
		void draw(Canvas canvas);
	}
}