using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			return async env => {
				Console.Write($"{env["owin.RequestMethod"]} {env["owin.RequestPath"]}");
				await next(env);
				var status = (int)env["owin.ResponseStatusCode"];
				var old = Console.ForegroundColor;
				switch(status) {
					case 200: Console.ForegroundColor = ConsoleColor.Green; break;
					case 401: Console.ForegroundColor = ConsoleColor.Cyan; break;
					case 404: Console.ForegroundColor = ConsoleColor.Yellow; break;
					case 405: Console.ForegroundColor = ConsoleColor.Yellow; break;
					case 500: Console.ForegroundColor = ConsoleColor.Red; break;
				}
				Console.WriteLine($" {status}");
				Console.ForegroundColor = old;
			};
		}
	}
}
