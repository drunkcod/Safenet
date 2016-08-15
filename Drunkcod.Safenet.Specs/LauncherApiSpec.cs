using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Cone;
using Drunkcod.Safenet.Simulator;
using Microsoft.Owin.Hosting;
using Owin;

namespace Drunkcod.Safenet.Simulator
{
	public class SafeSimStartup
	{
		public void Configure(IAppBuilder appBuilder, HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
			appBuilder.UseWebApi(config);
		}
	}

	public class LauncherApiController : ApiController
	{
		readonly HashSet<string> knownTokens;

		public LauncherApiController(HashSet<string> knownTokens)
		{
			this.knownTokens = knownTokens;
		}

		[HttpGet, Route("auth")]
		public HttpResponseMessage GetAuth() {
			var auth = Request.Headers.Authorization;
			if (auth != null && auth.Scheme == "Bearer" && knownTokens.Contains(auth.Parameter))
				return new HttpResponseMessage(HttpStatusCode.OK);
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		[HttpPost, Route("auth")]
		public object PostAuth() {
			var token = Guid.NewGuid().ToString();
			knownTokens.Add(token);
			return new { token };
		}
	}
}

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

		public void get_auth_without_token_is_unauthorized() {
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task can_check_token_validity() {
			var auth = await safe.AuthPostAsync(new SafenetAuthRequest
			{
				App = new SafenetAppInfo
				{
					Id = "TestApp",
					Name = "Testing",
					Vendor = "Test Inc",
					Version = "0.0.0.1"
				}
			});
			Check.That(
				() => auth.StatusCode == HttpStatusCode.OK,
				() => !string.IsNullOrEmpty(auth.Response.Token));
			safe.SetToken(auth.Response.Token);
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.OK);
		}
	}
}
