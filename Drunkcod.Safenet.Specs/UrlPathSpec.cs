using Cone;

namespace Drunkcod.Safenet.Specs
{
	[Describe(typeof(UrlPath))]
	public class UrlPathSpec
	{
		public static void combine_empty_root() {
			Check.That(() => UrlPath.Combine("", "app") == "app");
		}
	}
}
