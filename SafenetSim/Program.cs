using System;
using System.Collections.Generic;
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
			var fs = new SafenetInMemoryFileSystem();
			var apiHost = WebApp.Start(apiBaseAddress, app => {
				var config = new HttpConfiguration();
				var deps = new SimpleDependencyResolver(config.DependencyResolver);
				deps.Register<LauncherApiController>(() => new LauncherApiController(knownTokens, fs));
				var sim = new SafeSimStartup();
				app.Use((Func<Func<IDictionary<string,object>,Task>,Func<IDictionary<string,object>,Task>>)Log);
				sim.Configure(app, config);
				config.DependencyResolver = deps;
			});

			Console.ReadLine();
			apiHost.Dispose();
		}

		static Func<IDictionary<string,object>,Task> Log(Func<IDictionary<string,object>,Task> next) {
			return env => {
				Console.WriteLine($"{env["owin.RequestMethod"]} {env["owin.RequestPath"]}");
				return next(env);
			};
		}
	}
}
