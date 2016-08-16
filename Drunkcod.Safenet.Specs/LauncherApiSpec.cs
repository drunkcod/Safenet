﻿using System;
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

		[BeforeAll]
		public void StartSimulator() {
			apiHost = WebApp.Start(ApiBaseAddress, app => {
				var config = new HttpConfiguration();
				deps = new SimpleDependencyResolver(config.DependencyResolver);
				deps.Register<LauncherApiController>(() => new LauncherApiController(knownTokens, fs));
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
			await Authorize();
			Check.That(() => safe.AuthDeleteAsync().Result.StatusCode == HttpStatusCode.OK);
			Check.That(() => safe.AuthGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task dns_requires_auth() {
			Check.That(() => safe.DnsGetAsync().Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task nfs_app_directory_requires_auth() {
			Check.That(() => safe.NfsGetDirectoryAsync("app", string.Empty).Result.StatusCode == HttpStatusCode.Unauthorized);
		}

		public async Task nfs_app_root_is_private() {
			await Authorize();
			Check.With(() => safe.NfsGetDirectoryAsync("app", string.Empty).Result)
			.That(
				x => x.StatusCode == HttpStatusCode.OK,
				x => x.Response.Info.IsPrivate);
		}

		public async Task nfs_app_create_directory() {
			await Authorize();
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

		public async Task nfs_upload_file() {
			await Authorize();
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

		public async Task nfs_delete_file() {
			await Authorize();
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
			await Authorize();
			Check.That(() => safe.NfsDeleteFileAsync("app", "no-such.file").Result.StatusCode == HttpStatusCode.NotFound);
		}

		private async Task Authorize() {
			var auth = await safe.AuthPostAsync(MakeTestAppAuthRequest());
			Check.That(
				() => auth.StatusCode == HttpStatusCode.OK,
				() => !string.IsNullOrEmpty(auth.Response.Token));
			safe.SetToken(auth.Response.Token);
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
