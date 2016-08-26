namespace Drunkcod.Safenet
{
	public static class UrlPath
	{
		public static string GetDirectoryName(string url) {
			var n = url.LastIndexOf('/');
			return n == -1 
				? string.Empty 
				: url.Substring(0, n);
		}

		public static string Combine(string a, string b) {
			if(string.IsNullOrEmpty(a))
				return b;
			return a + "/" + b;
		}
	}
}