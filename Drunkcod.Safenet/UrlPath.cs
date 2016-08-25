namespace Drunkcod.Safenet
{
	static class UrlPath
	{
		public static string GetDirectoryName(string url) {
			var n = url.LastIndexOf('/');
			return n == -1 
				? string.Empty 
				: url.Substring(0, n);
		}
	}
}