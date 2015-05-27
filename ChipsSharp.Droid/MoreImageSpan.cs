using Android.Graphics.Drawables;
using Android.Text.Style;

namespace com.android.ex.chips
{
	/**
 * MoreImageSpan is a simple class created for tracking the existence of a
 * more chip across activity restarts/
 */

	internal class MoreImageSpan : ImageSpan
	{
		public MoreImageSpan(Drawable b)
			: base(b)
		{
		}
	}
}