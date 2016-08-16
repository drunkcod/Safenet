using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
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

		class SimpleDependencyResolver : IDependencyResolver
		{
			class SimpleDependencyScope : IDependencyScope
			{
				readonly SimpleDependencyResolver parent;
				readonly IDependencyScope inner;

				public SimpleDependencyScope(SimpleDependencyResolver parent, IDependencyScope inner) {
					this.parent = parent;
					this.inner = inner;
				}

				public void Dispose() => inner.Dispose();
 
				public object GetService(Type serviceType) {
					object found;
					return parent.TryResolveService(serviceType, out found) 
					? found 
					: inner.GetService(serviceType);
				}

				public IEnumerable<object> GetServices(Type serviceType) => inner.GetServices(serviceType);
			}

			readonly IDependencyResolver inner;
			readonly Dictionary<Type, Func<object>> knownTypes = new Dictionary<Type, Func<object>>();

			public SimpleDependencyResolver(IDependencyResolver inner) {
				this.inner = inner;
			}

			public void Register<T>(Func<object> makeObject) => knownTypes.Add(typeof(T), makeObject); 

			bool TryResolveService(Type type, out object instance) {
				Func<object> found;
				if (knownTypes.TryGetValue(type, out found)) {
					instance = found();
					return true;
				}
				instance = null;
				return false;
			}

			public void Dispose() => inner.Dispose();

			public object GetService(Type serviceType) {
				object found;
				if (TryResolveService(serviceType, out found))
					return found;
				return inner.GetService(serviceType);
			}

			public IEnumerable<object> GetServices(Type serviceType) => inner.GetServices(serviceType);

			public IDependencyScope BeginScope()
			{
				return new SimpleDependencyScope(this, inner.BeginScope());
			}
		}

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
