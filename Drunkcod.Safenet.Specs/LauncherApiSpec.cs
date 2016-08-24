using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
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

		readonly HashSet<string> knownTokens = new HashSet<string>();
		readonly SafenetInMemoryFileSystem fs = new SafenetInMemoryFileSystem();
		readonly SafenetInMemoryDns dns = new SafenetInMemoryDns();

		[BeforeAll]
		public void StartSimulator() {
			apiHost = WebApp.Start(ApiBaseAddress, app => {
				var config = new HttpConfiguration();
				deps = new SimpleDependencyResolver(config.DependencyResolver);
				deps.Register<LauncherApiController>(() => new LauncherApiController(knownTokens, fs, dns));
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
			fs.Clear();
			dns.Clear();
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
			await AuthorizeAsync();
			Check.That(() => safe.AuthDeleteAsync().Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public void dns_requires_auth() {
			Check.That(() => safe.DnsGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task dns_register_long_name() {
			await AuthorizeAsync();
			Check.That(() => safe.DnsPostAsync("example").Result.StatusCode == HttpStatusCode.OK);
			Check.With(() => safe.DnsGetAsync().Result)
			.That(dns => dns.Response.Contains("example"));
		}

		public async Task dns_unregister_long_name() {
			await AuthorizeAsync();
			Check.That(() => safe.DnsPostAsync("example").Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.DnsDeleteAsync("example").Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.DnsGetAsync().Result.Response.Length == 0);
		}

		public async Task dns_get_services_by_long_name() {
			await AuthorizeAsync();
			Check.That(() => safe.DnsPostAsync("example").Result.StatusCode == HttpStatusCode.OK);

			var getServices = safe.DnsGetAsync("example").Result; 
			Check.That(() => getServices.StatusCode == HttpStatusCode.OK);
			Check.That(() => getServices.Response.Length == 0);
		}

		public async Task dns_add_service_to_long_name() {
			await AuthorizeAsync();
			Check.That(() => safe.DnsPostAsync("example").Result.StatusCode == HttpStatusCode.OK);

			Check.That(() => safe.DnsPutAsync(new SafenetDnsRegisterServiceRequest {
				RootPath = "app",
				LongName = "example",
				ServiceName = "www",
				ServiceHomeDirPath = "/www"
			}).Result.StatusCode == HttpStatusCode.OK);

			var getServices = safe.DnsGetAsync("example").Result; 
			Check.That(() => getServices.StatusCode == HttpStatusCode.OK);
			Check.That(() => getServices.Response.Length == 1);
			Check.That(() => getServices.Response[0] == "www");
		}

		public async Task dns_delete_service()
		{
			await AuthorizeAsync();
			CreateService("www", "example");

			Check.That(() => safe.DnsDeleteAsync("www", "example").Result.StatusCode == HttpStatusCode.OK);

			var getServices = safe.DnsGetAsync("example").Result; 
			Check.That(() => getServices.StatusCode == HttpStatusCode.OK);
			Check.That(() => getServices.Response.Length == 0);
		}

		private void CreateService(string serviceName, string longName)
		{
			Check.That(() => safe.DnsPostAsync("example").Result.StatusCode == HttpStatusCode.OK);

			Check.That(() => safe.DnsPutAsync(new SafenetDnsRegisterServiceRequest
			{
				RootPath = "app",
				LongName = longName,
				ServiceName = serviceName,
				ServiceHomeDirPath = "/www"
			}).Result.StatusCode == HttpStatusCode.OK);
		}

		public void nfs_app_directory_requires_auth() {
			Check.That(() => safe.NfsGetDirectoryAsync("app", string.Empty).Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task nfs_app_root_is_private() {
			await AuthorizeAsync();
			Check.With(() => safe.NfsGetDirectoryAsync("app", string.Empty).Result)
			.That(
				x => x.StatusCode == HttpStatusCode.OK,
				x => x.Response.Info.IsPrivate);
		}

		public async Task nfs_app_create_directory() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
				RootPath = "app",
				DirectoryPath = "test",
				IsPrivate = false,
			}).Result.StatusCode == HttpStatusCode.OK);

			//Visible from app root?
			Check.With(() => safe.NfsGetDirectoryAsync("app", string.Empty).Result)
			.That(
				x => x.StatusCode == HttpStatusCode.OK,
				x => x.Response.SubDirectories.Any(dir => dir.Name == "test"));

			Check.With(() => safe.NfsGetDirectoryAsync("app", "test").Result)
			.That(
				x => x.StatusCode == HttpStatusCode.OK,
				x => x.Response.Info.Name == "test",
				x => x.Response.Info.IsPrivate == false);
		}

		public async Task nfs_check_directory_exists() {
			await AuthorizeAsync();

			Check.That(() => safe.NfsHeadDirectoryAsync("app", "test").Result == HttpStatusCode.NotFound);

			CreateDirectory("app", "test");

			Check.That(() => safe.NfsHeadDirectoryAsync("app", "test").Result == HttpStatusCode.OK);
		}

		public async Task nfs_delete_directory() {
			await AuthorizeAsync();
			CreateDirectory("app", "test");

			Check.That(() => safe.NfsDeleteDirectoryAsync("app", "test").Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => !safe.DirectoryExists("app", "test"));

		}

		public async Task nfs_get_subdirectory() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
				RootPath = "app",
				DirectoryPath = "parent/child",
				IsPrivate = true,
			}).Result.StatusCode == HttpStatusCode.OK);

			var result = await safe.NfsGetDirectoryAsync("app", "parent/child");
			Check.That(() => result.StatusCode == HttpStatusCode.OK);
			Check.With(() => result.Response)
			.That(
				x => x.Info.Name == "child",
				x => x.Info.IsPrivate);
		}

		public async Task nfs_upload_file() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsPostAsync(new SafenetNfsPutFileRequest {
				RootPath = "app",
				FilePath = "test.txt",
				ContentType = MediaTypeHeaderValue.Parse("text/plain"),
				Bytes = Encoding.UTF8.GetBytes("Hello World")
			}).Result.StatusCode == HttpStatusCode.OK);

			Check.With(() => safe.NfsGetDirectoryAsync("app", "").Result)
			.That(
				x => x.StatusCode == HttpStatusCode.OK,
				x => x.Response.Files.Any(file => file.Name == "test.txt"),
				x => x.Response.Files.Single(file => file.Name == "test.txt").Size == 11,
				x => x.Response.Files.All(file => file.CreatedOn != default(DateTime)));
			
			var theFile = await safe.NfsGetFileAsync("app", "test.txt");
			Check.That(() => theFile.StatusCode == HttpStatusCode.OK);
			Check.With(() => theFile.Response)
			.That(
				file => file.ContentType == MediaTypeHeaderValue.Parse("text/plain"),
				file => file.ContentLength == 11,
				file => file.CreatedOn != default(DateTime),
				file => new StreamReader(file.Body).ReadToEnd() == "Hello World"
			);
		}

		public async Task nfs_upload_file_bad_request_if_exists() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsPostAsync(TextFile()).Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.NfsPostAsync(TextFile()).Result.StatusCode == HttpStatusCode.BadRequest);
		}

		private static SafenetNfsPutFileRequest TextFile()
		{
			return new SafenetNfsPutFileRequest {
				RootPath = "app",
				FilePath = "test.txt",
				ContentType = MediaTypeHeaderValue.Parse("text/plain"),
				Bytes = Encoding.UTF8.GetBytes("Hello World")
			};
		}

		public async Task nfs_delete_file() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsPostAsync(new SafenetNfsPutFileRequest {
				RootPath = "app",
				FilePath = "test.txt",
				ContentType = MediaTypeHeaderValue.Parse("text/plain"),
				Bytes = Encoding.UTF8.GetBytes("Hello World")
			}).Result.StatusCode == HttpStatusCode.OK);

			Check.That(() => safe.NfsDeleteFileAsync("app", "test.txt").Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.NfsGetFileAsync("app", "test.txt").Result.StatusCode == HttpStatusCode.NotFound);
		}

		public async Task nfs_delete_missing_file_is_not_found() {
			await AuthorizeAsync();
			Check.That(() => safe.NfsDeleteFileAsync("app", "no-such.file").Result.StatusCode == HttpStatusCode.NotFound);
		}

		private async Task AuthorizeAsync() {
			var auth = await safe.AuthPostAsync(MakeTestAppAuthRequest());
			Check.That(
				() => auth.StatusCode == HttpStatusCode.OK,
				() => !string.IsNullOrEmpty(auth.Response.Token));
			safe.SetToken(auth.Response.Token);
		}

		void CreateDirectory(string rootPath, string directoryPath) {
			Assume.That(() => safe.NfsPostAsync(new SafenetNfsCreateDirectoryRequest {
				RootPath = rootPath,
				DirectoryPath = directoryPath,
				IsPrivate = false,
			}).Result.StatusCode == HttpStatusCode.OK);
		}

		static SafenetAuthRequest MakeTestAppAuthRequest() =>
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
