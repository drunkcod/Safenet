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

		public Task<SafenetResponse> AuthGetAsync() =>
			EmptyResponseAsync(http.GetAsync("/auth")); 

		public Task<SafenetResponse<SafenetAuthResponse>> AuthPostAsync(SafenetAuthRequest auth) =>
			ReadResponseAsync<SafenetAuthResponse>(http.PostAsync("/auth", ToPayload(auth)));

		public Task<SafenetResponse> AuthDeleteAsync() =>
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

		public Task<SafenetResponse> DnsPostAsync(string longName) =>
			EmptyResponseAsync(http.PostAsync($"/dns/{longName}", new StringContent(string.Empty)));

		public Task<SafenetResponse> DnsPostAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PostAsync("/dns", ToPayload(service)));

		public Task<SafenetResponse> DnsPutAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PutAsync("/dns", ToPayload(service)));

		public Task<SafenetResponse> DnsDeleteAsync(string longName) =>
			EmptyResponseAsync(http.DeleteAsync($"/dns/{longName}"));

		public Task<SafenetResponse> DnsDeleteAsync(string serviceName, string longName) =>
			EmptyResponseAsync(http.DeleteAsync($"/dns/{serviceName}/{longName}"));

		public Task<SafenetResponse<SafenetDirectoryResponse>> NfsGetDirectoryAsync(string rootPath, string directoryPath) =>
			ReadResponseAsync<SafenetDirectoryResponse>(http.GetAsync($"/nfs/directory/{rootPath}/{directoryPath}")); 

		public async Task<HttpStatusCode> NfsHeadDirectoryAsync(string rootPath, string directoryPath) =>
			(await http.SendAsync(new HttpRequestMessage(HttpMethod.Head, $"/nfs/directory/{rootPath}/{directoryPath}"))).StatusCode;

		public Task<SafenetResponse<SafenetFileResponse>> NfsGetFileAsync(string rootPath, string filePath) => 
			FileResponseAsync(http.GetAsync($"/nfs/file/{rootPath}/{WebUtility.UrlEncode(filePath)}")); 

		public Task<SafenetResponse> NfsPostAsync(SafenetNfsCreateDirectoryRequest directory) =>
			EmptyResponseAsync(http.PostAsync($"/nfs/directory/{directory.RootPath}/{directory.DirectoryPath}", ToPayload(new {
				isPrivate = directory.IsPrivate,
				metadata = Convert.ToBase64String(directory.Metadata),
			})));

		public Task<SafenetResponse> NfsPostAsync(SafenetNfsPutFileRequest file) {
			var body = new ByteArrayContent(file.Bytes);
			body.Headers.ContentType = file.ContentType;
			body.Headers.Add("Metadata", Convert.ToBase64String(file.Metadata));
			return EmptyResponseAsync(http.PostAsync($"/nfs/file/{file.RootPath}/{file.FilePath}", body));
		}

		public Task<SafenetResponse> NfsDeleteFileAsync(string rootPath, string filePath) =>
			EmptyResponseAsync(http.DeleteAsync($"/nfs/file/{rootPath}/{filePath}")); 
		
		async Task<SafenetResponse<SafenetFileResponse>> FileResponseAsync(Task<HttpResponseMessage> request) =>
			await SafenetResponse.CreateAsync(
				await request.ConfigureAwait(false),
				async r => new SafenetFileResponse {
					CreatedOn = DateTime.Parse(r.Headers.GetValues("created-on").Single()),
					ModifiedOn = DateTime.Parse(r.Content.Headers.GetValues("last-modified").Single()),
					ContentLength = r.Content.Headers.ContentLength,
					ContentType = r.Content.Headers.ContentType,
					ContentRange = r.Content.Headers.ContentRange,
					Metadata = HeaderOrDefault(r.Headers, "metadata", Convert.FromBase64String, new byte[0]),
					Body = await r.Content.ReadAsStreamAsync().ConfigureAwait(false)
				},
				r => ReadErrorAsync(r.Content));

		async Task<SafenetResponse<T>> ReadResponseAsync<T>(Task<HttpResponseMessage> request) =>
			await SafenetResponse.CreateAsync(
				await request.ConfigureAwait(false),
				r => DeserializeAsync<T>(r.Content),
				r => ReadErrorAsync(r.Content));
		

		async Task<SafenetResponse> EmptyResponseAsync(Task<HttpResponseMessage> request) {
			var r = await request.ConfigureAwait(false);
			return new SafenetResponse(r.StatusCode, r.IsSuccessStatusCode 
				? (SafenetError?)null
				: await ReadErrorAsync(r.Content).ConfigureAwait(false));
		}

		async Task<SafenetError> ReadErrorAsync(HttpContent content) =>
			content.Headers.ContentType?.MediaType == "application/json"
			? await DeserializeAsync<SafenetError>(content)
			: new SafenetError { Description = await content.ReadAsStringAsync().ConfigureAwait(false) };

		static T HeaderOrDefault<T>(HttpHeaders headers, string header, Func<string, T> convert, T defaultValue) {
			IEnumerable<string> found;
			return headers.TryGetValues(header, out found) 
			? convert(found.Single()) 
			: defaultValue;
		}

		async Task<T> DeserializeAsync<T>(HttpContent body) {
			using(var jr = new JsonTextReader(new StreamReader(await body.ReadAsStreamAsync().ConfigureAwait(false))))
				return json.Deserialize<T>(jr);
		}
	
		HttpContent ToPayload(object obj) => new JsonContent(json, obj);
	}
}