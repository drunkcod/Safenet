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
		readonly SafenetInMemoryDns dns;

		public LauncherApiController(HashSet<string> knownTokens, SafenetInMemoryFileSystem fs, SafenetInMemoryDns dns) {
			this.knownTokens = knownTokens;
			this.fs = fs;
			this.dns = dns;
		}

		[HttpGet, Route("auth")]
		public IHttpActionResult AuthGet() {
			Authorize();
			return Ok();
		}

		[HttpPost, Route("auth")]
		public SafenetAuthResponse AuthPost() {
			var token = Guid.NewGuid().ToString();
			knownTokens.Add(token);
			return new SafenetAuthResponse { Token = token };
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
			return dns.GetServices().Select(x => x.Key).ToArray();
		}

		[HttpPut, Route("dns")]
		public HttpResponseMessage DnsAddSerivce([FromBody] SafenetDnsRegisterServiceRequest input) {
			Authorize();
			dns.BindService(input.LongName, input.ServiceName);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpGet, Route("dns/{longName}")]
		public string[] DnsGet(string longName) {
			Authorize();
			return dns.GetServices(longName);
		}

		[HttpPost, Route("dns/{longName}")]
		public HttpResponseMessage DnsRegister(string longName) {
			Authorize();
			dns.Register(longName);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpDelete, Route("dns/{longName}")]
		public HttpResponseMessage DnsDelete(string longName) {
			Authorize();
			dns.Unregister(longName);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpDelete, Route("dns/{serviceName}/{longName}")]
		public HttpResponseMessage DnsDelete(string serviceName, string longName) {
			Authorize();
			dns.RemoveService(serviceName, longName);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpGet, Route("nfs/directory/{root}/{*directory?}")]
		public SafenetDirectoryResponse NfsGetDirectory(string root, string directory = "") {
			Authorize();
			var dir = fs.GetOrCreateDirectory(Path.Combine(root, directory));
			return new SafenetDirectoryResponse {
				Info = dir.Info,
				SubDirectories = dir.SubDirectories.Select(x => x.Info).ToArray(),
				Files = dir.Files.Select(x => new SafenetFileInfo {
					Name = x.Name,
					Size = x.Bytes.Length,
					CreatedOn = x.CreatedOn,
					ModifiedOn = x.ModifiedOn,
					Metadata = x.Metadata,
				}).ToArray(),
			};
		}

		[HttpHead, Route("nfs/directory/{root}/{*directory?}")]
		public HttpResponseMessage NfsHeadDirectory(string root, string directory = "") {
			Authorize();
			return new HttpResponseMessage(fs.DirectoryExists(Path.Combine(root, directory)) ? HttpStatusCode.OK : HttpStatusCode.NotFound);
		}

		[HttpPost, Route("nfs/directory/{root}/{*directory}")]
		public HttpResponseMessage NfsCreateDirectory(string root, string directory, [FromBody] SafenetNfsCreateDirectoryRequest dir) {
			var newDir = fs.GetOrCreateDirectory(Path.Combine(root, directory));
			newDir.Info.IsPrivate = dir.IsPrivate;
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpDelete, Route("nfs/directory/{root}/{*directory?}")]
		public HttpResponseMessage NfsDeleteDirectory(string root, string directory) {
			fs.DeleteDirectory(Path.Combine(root, directory));
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		[HttpGet, Route("nfs/file/{root}/{*path}")]
		public HttpResponseMessage NfsGetFile(string root, string path) {
			var sourceDir = fs.GetOrCreateDirectory(Path.Combine(root, Path.GetDirectoryName(path)));
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
			var targetDir = fs.GetOrCreateDirectory(Path.Combine(root, Path.GetDirectoryName(path)));
			if(targetDir.TryCreateFile(new SafenetInMemorFile {
				Name = Path.GetFileName(path),
				MediaType = Request.Content.Headers.ContentType,
				CreatedOn = DateTime.UtcNow,
				ModifiedOn = DateTime.UtcNow,
				Bytes = await Request.Content.ReadAsByteArrayAsync()
			}))
				return new HttpResponseMessage(HttpStatusCode.OK);
			return new HttpResponseMessage(HttpStatusCode.BadRequest);
		}

		[HttpDelete, Route("nfs/file/{root}/{*path}")]
		public HttpResponseMessage NfsDeleteFile(string root, string path) {
			var targetDir = fs.GetOrCreateDirectory(Path.Combine(root,Path.GetDirectoryName(path)));
			
			if(!targetDir.DeleteFile(Path.GetFileName(path)))
				return new HttpResponseMessage(HttpStatusCode.NotFound);
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