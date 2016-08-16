using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Drunkcod.Safenet.Simulator.Controllers
{
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
			var dir = fs.GetOrCreateDirectory(directory);
			return new SafenetDirectoryResponse {
				Info = dir.Info,
				SubDirectories = dir.SubDirectories.Select(x => x.Info).ToArray(),
				Files = dir.Files.Select(x => new SafenetFileInfo {
					Name = x.Name,
					Size = x.Bytes.Length,
					CreatedOn = x.CreatedOn,
					ModifiedOn = x.ModifiedOn,
				}).ToArray(),
			};
		}

		[HttpPost, Route("nfs/directory/{root}/{directory}")]
		public HttpResponseMessage NfsCreateDirectory(string root, string directory, [FromBody] SafenetNfsCreateDirectoryRequest dir) {
			var rootDir = fs.GetOrCreateDirectory(string.Empty);
			var newDir = fs.GetOrCreateDirectory(directory);
			newDir.Info.IsPrivate = dir.IsPrivate;
			rootDir.SubDirectories.Add(new SafenetInMemoryDirectory {
				Info = newDir.Info
			});
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpGet, Route("nfs/file/{root}/{*path}")]
		public HttpResponseMessage NfsGetFile(string root, string path) {
			var sourceDir = fs.GetOrCreateDirectory(Path.GetDirectoryName(path));
			var file = sourceDir.Files.SingleOrDefault(x => x.Name == Path.GetFileName(path));
			if(file == null)
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			var r = new HttpResponseMessage(HttpStatusCode.OK) {
				Content = new ByteArrayContent(file.Bytes),
			};
			r.Headers.Add("created-on", file.CreatedOn.ToString("r"));
			r.Content.Headers.Add("last-modified", file.ModifiedOn.ToString("r"));
			r.Content.Headers.ContentType = file.MediaType;
			return r;
		}

		[HttpPost, Route("nfs/file/{root}/{*path}")]
		public async Task<HttpResponseMessage> NfsPutFileAsync(string root, string path) {
			var targetDir = fs.GetOrCreateDirectory(Path.GetDirectoryName(path));
			targetDir.Files.Add(new SafenetInMemorFile {
				Name = Path.GetFileName(path),
				MediaType = Request.Content.Headers.ContentType,
				CreatedOn = DateTime.UtcNow,
				ModifiedOn = DateTime.UtcNow,
				Bytes = await Request.Content.ReadAsByteArrayAsync()
			});
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpDelete, Route("nfs/file/{root}/{*path}")]
		public HttpResponseMessage NfsDeleteFile(string root, string path) {
			var targetDir = fs.GetOrCreateDirectory(Path.GetDirectoryName(path));
			targetDir.Files.RemoveAt(targetDir.Files.FindIndex(x => x.Name == Path.GetFileName(path)));
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