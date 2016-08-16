using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Cone;
using Drunkcod.Safenet.Simulator;
using Drunkcod.Safenet.Simulator.Controllers;
using Microsoft.Owin.Hosting;

namespace Drunkcod.Safenet.Specs
{
	[Feature("LauncherAPI")]
	public class LauncherApiSpec
	{
		const string ApiBaseAddress = "http://localhost:9000/";
		IDisposable apiHost;
		SafenetClient safe;

		SimpleDependencyResolver deps;
		HashSet<string> knownTokens = new HashSet<string>();

		[BeforeAll]
		public void StartSimulator() {
			apiHost = WebApp.Start(ApiBaseAddress, app => {
				var config = new HttpConfiguration();
				deps = new SimpleDependencyResolver(config.DependencyResolver);
				deps.Register<LauncherApiController>(() => new LauncherApiController(knownTokens));
				var sim = new SafeSimStartup();
				sim.Configure(app, config);
				config.DependencyResolver = deps;
			});
		}

		[AfterAll]
		public void Shutdown() => apiHost.Dispose();

		[BeforeEach]
		public void NewSafenetClient() {
			safe = new SafenetClient(ApiBaseAddress);
		}

		public void auth_without_token_is_unauthorized() {
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task auth_check_token_validity() {
			var auth = await safe.AuthPostAsync(MakeTestAppAuthRequest());
			Check.That(
				() => auth.StatusCode == HttpStatusCode.OK,
				() => !string.IsNullOrEmpty(auth.Response.Token));
			safe.SetToken(auth.Response.Token);
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.OK);
		}

		public async Task auth_expire_token() {
			var auth = await safe.AuthPostAsync(MakeTestAppAuthRequest());
			Check.That(
				() => auth.StatusCode == HttpStatusCode.OK,
				() => !string.IsNullOrEmpty(auth.Response.Token));
			safe.SetToken(auth.Response.Token);
			Check.That(() => safe.AuthDeleteAsync().Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		private static SafenetAuthRequest MakeTestAppAuthRequest() =>
			new SafenetAuthRequest {
				App = new SafenetAppInfo {
					Id = "TestApp",
					Name = "Testing",
					Vendor = "Test Inc",
					Version = "0.0.0.1"
				}
			};
	}
}
