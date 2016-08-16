using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace Drunkcod.Safenet.Simulator
{
	class SafenetSimStartup
	{
	}
}
namespace Drunkcod.Safenet.Simulator
{
	public class SafeSimStartup
	{
		public void Configure(IAppBuilder appBuilder, HttpConfiguration config)
		{
			config.Formatters.Remove(config.Formatters.XmlFormatter);
			config.MapHttpAttributeRoutes();
			appBuilder.UseWebApi(config);
		}
	}
}
