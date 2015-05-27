using Android.Graphics;
using Android.OS;

namespace com.android.ex.chips
{
	public class ChipsUtil
	{
		public static bool supportsChipsUi()
		{
			return Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
		}

		public static Bitmap getClip(Bitmap bitmap)
		{
			var width = bitmap.Width;
			var height = bitmap.Height;

			var output = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

			var canvas = new Canvas(output);
			var paint = new Paint();
			var rect = new Rect(0, 0, width, height);

			paint.AntiAlias = true;
			canvas.DrawARGB(0, 0, 0, 0);
			canvas.DrawCircle(width / (float)2, height / (float)2, width / (float)2, paint);
			paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
			canvas.DrawBitmap(bitmap, null, rect, paint);
			return output;
		}
	}
}