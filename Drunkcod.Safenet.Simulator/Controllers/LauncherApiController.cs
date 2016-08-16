using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;

namespace Drunkcod.Safenet.Simulator.Controllers
{
	public class SafenetInMemoryFileSystem
	{
		readonly ConcurrentDictionary<string, SafenetDirectoryResponse> directories = new ConcurrentDictionary<string, SafenetDirectoryResponse>();

		public SafenetDirectoryResponse GetOrCreateDirectory(string path) {
			return directories.GetOrAdd(path, _ => new SafenetDirectoryResponse
			{
				Info = new SafenetDirectoryInfo {
					CreatedOn = DateTime.UtcNow,
					ModifiedOn = DateTime.UtcNow,
					IsPrivate = true,
					IsVersioned = false,
					Name = path,
				}
			});
		}
	} 

	public class LauncherApiController : ApiController
	{
		readonly HashSet<string> knownTokens;
		readonly SafenetInMemoryFileSystem fs;

		public LauncherApiController(HashSet<string> knownTokens, SafenetInMemoryFileSystem fs) {
			this.knownTokens = knownTokens;
			this.fs = fs;
		}

		[HttpGet, Route("auth")]
		public IHttpActionResult AuthGet() {
			Authorize();
			return Ok();
		}

		[HttpPost, Route("auth")]
		public object AuthPost() {
			var token = Guid.NewGuid().ToString();
			knownTokens.Add(token);
			return new { token };
		}

		[HttpDelete, Route("auth")]
		public HttpResponseMessage AuthDelete() {
			var auth = Request.Headers.Authorization;
			if (IsAuthorized(auth)) {
				knownTokens.Remove(auth.Parameter);
				return new HttpResponseMessage(HttpStatusCode.OK);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		[HttpGet, Route("dns")]
		public string[] DnsGet() {
			Authorize();
			return new string[0];
		}

		[HttpGet, Route("nfs/directory/{root}/{directory?}")]
		public SafenetDirectoryResponse NfsGetDirectory(string root, string directory = "") {
			Authorize();
			return fs.GetOrCreateDirectory(directory);
		}

		[HttpPost, Route("nfs/directory/{root}/{directory}")]
		public HttpResponseMessage NfsCreateDirectory(string root, string directory, [FromBody] SafenetNfsCreateDirectoryRequest dir) {
			var rootDir = fs.GetOrCreateDirectory(string.Empty);
			var newDir = fs.GetOrCreateDirectory(directory);
			newDir.Info.IsPrivate = dir.IsPrivate;
			rootDir.SubDirectories = rootDir.SubDirectories.Concat(new[] {
				newDir.Info
			}).ToArray();
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		void Authorize() {
			if(!IsAuthorized(Request.Headers.Authorization))
				throw new HttpResponseException(HttpStatusCode.Unauthorized);
		}

		bool IsAuthorized(AuthenticationHeaderValue auth) =>
			auth != null && auth.Scheme == "Bearer" && knownTokens.Contains(auth.Parameter);
	}
}