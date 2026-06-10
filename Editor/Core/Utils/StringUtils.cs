namespace Wireframe
{
	public static class StringUtils
	{
		public static string HideText(this string text, string toHide)
		{
			if (string.IsNullOrEmpty(toHide))
			{
				return text;
			}

			text = text.Replace(toHide, "****");

			return text;
		}
		
		public static string HideText(this string text, string[] toHide)
		{
			if (toHide == null || toHide.Length == 0)
			{
				return text;
			}

			foreach (string hide in toHide)
			{
				text = text.Replace(hide, "****");
			}

			return text;
		}
	}
}