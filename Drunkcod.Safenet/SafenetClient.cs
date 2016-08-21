using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Drunkcod.Safenet
{
	public class SafenetClient
	{
		readonly HttpClient http;
		readonly JsonSerializer json;
	
		public SafenetClient(string apiRoot = "http://localhost:8100") {
			this.http = new HttpClient {
				BaseAddress = new Uri(apiRoot),
			};
			this.json = new JsonSerializer();
		}

		public void SetToken(string token) {
			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		public Task<SafenetEmptyResponse> AuthGetAsync() =>
			EmptyResponseAsync(http.GetAsync("/auth")); 

		public Task<SafenetResponse<SafenetAuthResponse>> AuthPostAsync(SafenetAuthRequest auth) =>
			ReadResponseAsync<SafenetAuthResponse>(http.PostAsync("/auth", ToPayload(auth)));

		public Task<SafenetEmptyResponse> AuthDeleteAsync() =>
			EmptyResponseAsync(http.DeleteAsync("/auth")); 

		public Task<SafenetResponse<string[]>> DnsGetAsync() =>
			ReadResponseAsync<string[]>(http.GetAsync("/dns"));

		public Task<SafenetResponse<string[]>> DnsGetAsync(string longName) =>
			ReadResponseAsync<string[]>(http.GetAsync($"/dns/{longName}"));

		public Task<SafenetResponse<SafenetDirectoryResponse>> DnsGetAsync(string service, string longName) =>
			ReadResponseAsync<SafenetDirectoryResponse>(http.GetAsync($"/dns/{service}/{longName}"));

		public Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path) =>
			FileResponseAsync(http.GetAsync($"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}"));

		public Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path, RangeHeaderValue range) {
			var rm = new HttpRequestMessage(HttpMethod.Get, $"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}");
			rm.Headers.Range = range;
			return FileResponseAsync(http.SendAsync(rm));
		}

		public Task<SafenetEmptyResponse> DnsPostAsync(string longName) =>
			EmptyResponseAsync(http.PostAsync($"/dns/{longName}", new StringContent(string.Empty)));

		public Task<SafenetEmptyResponse> DnsPostAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PostAsync("/dns", ToPayload(service)));

		public Task<SafenetEmptyResponse> DnsPutAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PutAsync("/dns", ToPayload(service)));

		public Task<SafenetEmptyResponse> DnsDeleteAsync(string longName) =>
			EmptyResponseAsync(http.DeleteAsync($"/dns/{longName}"));

		public Task<SafenetResponse<SafenetDirectoryResponse>> NfsGetDirectoryAsync(string rootPath, string directoryPath) =>
			ReadResponseAsync<SafenetDirectoryResponse>(http.GetAsync($"/nfs/directory/{rootPath}/{directoryPath}")); 

		public Task<SafenetResponse<SafenetFileResponse>> NfsGetFileAsync(string rootPath, string filePath) => 
			FileResponseAsync(http.GetAsync($"/nfs/file/{rootPath}/{WebUtility.UrlEncode(filePath)}")); 

		public Task<SafenetEmptyResponse> NfsPostAsync(SafenetNfsCreateDirectoryRequest directory) =>
			EmptyResponseAsync(http.PostAsync($"/nfs/directory/{directory.RootPath}/{directory.DirectoryPath}", ToPayload(new {
				isPrivate = directory.IsPrivate,
				metadata = Convert.ToBase64String(directory.Metadata),
			})));

		public Task<SafenetEmptyResponse> NfsPostAsync(SafenetNfsPutFileRequest file) {
			var body = new ByteArrayContent(file.Bytes);
			body.Headers.ContentType = file.ContentType;
			body.Headers.Add("Metadata", Convert.ToBase64String(file.Metadata));
			return EmptyResponseAsync(http.PostAsync($"/nfs/file/{file.RootPath}/{file.FilePath}", body));
		}

		public Task<SafenetEmptyResponse> NfsDeleteFileAsync(string rootPath, string filePath) =>
			EmptyResponseAsync(http.DeleteAsync($"/nfs/file/{rootPath}/{filePath}")); 

		async Task<SafenetResponse<SafenetFileResponse>> FileResponseAsync(Task<HttpResponseMessage> request) {
			var r = await request.ConfigureAwait(false);
			var response = new SafenetResponse<SafenetFileResponse>(r.StatusCode);
			if (r.IsSuccessStatusCode)
				response.Response = new SafenetFileResponse {
					CreatedOn = DateTime.Parse(r.Headers.GetValues("created-on").Single()),
					ModifiedOn = DateTime.Parse(r.Content.Headers.GetValues("last-modified").Single()),
					ContentLength = r.Content.Headers.ContentLength,
					ContentType = r.Content.Headers.ContentType,
					ContentRange = r.Content.Headers.ContentRange,
					Metadata = HeaderOrDefault(r.Headers, "metadata", Convert.FromBase64String, new byte[0]),
					Body = await r.Content.ReadAsStreamAsync().ConfigureAwait(false)
				};
			else
				response.Error = await ReadErrorAsync(r.Content).ConfigureAwait(false);
			return response;
		}

		async Task<SafenetEmptyResponse> EmptyResponseAsync(Task<HttpResponseMessage> request) {
			var r = await request.ConfigureAwait(false);
			return new SafenetEmptyResponse(r.StatusCode, r.IsSuccessStatusCode 
				? (SafenetError?)null
				: await ReadErrorAsync(r.Content).ConfigureAwait(false));
		}

		async Task<SafenetResponse<T>> ReadResponseAsync<T>(Task<HttpResponseMessage> request) {
			var r = await request.ConfigureAwait(false);
			var response = new SafenetResponse<T>(r.StatusCode);
			if (r.IsSuccessStatusCode)
				response.Response = Deserialize<T>(await r.Content.ReadAsStreamAsync().ConfigureAwait(false));
			else
				response.Error = await ReadErrorAsync(r.Content).ConfigureAwait(false);
			return response;
		}

		async Task<SafenetError> ReadErrorAsync(HttpContent content) {
			return content.Headers.ContentType?.MediaType == "application/json"
				? Deserialize<SafenetError>(await content.ReadAsStreamAsync().ConfigureAwait(false))
				: new SafenetError { Description = await content.ReadAsStringAsync().ConfigureAwait(false) };
		}

		static T HeaderOrDefault<T>(HttpHeaders headers, string header, Func<string, T> convert, T defaultValue) {
			IEnumerable<string> found;
			if(headers.TryGetValues(header, out found))
				return convert(found.Single());
			return defaultValue;
		}

		T Deserialize<T>(Stream body) {
			using(var jr = new JsonTextReader(new StreamReader(body)))
				return json.Deserialize<T>(jr);
		}
	
		HttpContent ToPayload(object obj) => new JsonContent(json, obj);
	}
}