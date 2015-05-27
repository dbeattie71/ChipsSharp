namespace com.android.ex.chips
{
	public static class Extensions
	{
		public static string SubString(this string s, int start, int end)
		{
			return s.Substring(start, end - start);
		}
	}
}