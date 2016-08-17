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
				var req =  $"{env["owin.RequestMethod"]} {env["owin.RequestPath"]}";
				var n = WithConsole(_ => {
					Write(ConsoleColor.DarkGray, req); 
					Console.WriteLine();
				});
				await next(env);
				var status = (int)env["owin.ResponseStatusCode"];
				var c = HttpStatusToColor(status);

				WithConsole(lid => {
					if(lid == n + 1)
						Console.CursorTop -= 1 + req.Length / Console.WindowWidth;	
					Write(c, env["owin.RequestMethod"].ToString());
					Console.Write(' ');
					Console.Write(env["owin.RequestPath"]);
					Console.Write(' ');
					Write(ConsoleColor.DarkGray, $"{status}");
					Console.WriteLine();
				});
			};
		}

		private static ConsoleColor HttpStatusToColor(int status) {
			switch (status) {
				case 200: return ConsoleColor.Green;
				case 401: return ConsoleColor.Cyan;
				case 404: return ConsoleColor.Yellow;
				case 405: return ConsoleColor.Yellow;
				case 500: return ConsoleColor.Red;
			}
			return Console.ForegroundColor;
		}

		static int lineId = 0;
		static int WithConsole(Action<int> write) {
			lock(Console.Out) {
				write(++lineId);
				return lineId;
			}
		}

		static void Write(ConsoleColor fg, string value) {
			var old = Console.ForegroundColor;
			Console.ForegroundColor = fg;
			Console.Write(value);
			Console.ForegroundColor = old;
		}
	}
}
