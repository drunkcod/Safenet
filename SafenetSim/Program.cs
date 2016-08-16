using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Drunkcod.Safenet.Simulator;
using Drunkcod.Safenet.Simulator.Controllers;
using Microsoft.Owin.Hosting;

namespace SafenetSim
{
	class Program
	{
		static void Main(string[] args) {
			var apiBaseAddress = args[0];
			var knownTokens = new HashSet<string>();
			var apiHost = WebApp.Start(apiBaseAddress, app => {
				var config = new HttpConfiguration();
				var deps = new SimpleDependencyResolver(config.DependencyResolver);
				deps.Register<LauncherApiController>(() => new LauncherApiController(knownTokens));
				var sim = new SafeSimStartup();
				sim.Configure(app, config);
				config.DependencyResolver = deps;
			});

			Console.ReadLine();
			apiHost.Dispose();
		}
	}
}
