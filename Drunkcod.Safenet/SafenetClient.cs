using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

		public Task<SafenetResponse<string[]>> DnsGetAsync() =>
			MakeResponseAsync<string[]>(http.GetAsync("/dns"));

		public Task<SafenetResponse<string[]>> DnsGetAsync(string longName) =>
			MakeResponseAsync<string[]>(http.GetAsync($"/dns/{longName}"));

		public Task<SafenetResponse<SafenetDirectoryResponse>> DnsGetAsync(string service, string longName) =>
			MakeResponseAsync<SafenetDirectoryResponse>(http.GetAsync($"/dns/{service}/{longName}"));

		public Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path) {
			var request = http.GetAsync($"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}");
			return MakeFileResponse(request);
		}

		public Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path, RangeHeaderValue range) {
			var rm = new HttpRequestMessage(HttpMethod.Get, $"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}");
			rm.Headers.Range = range;
			return MakeFileResponse(http.SendAsync(rm));
		}

		public Task<SafenetResponse<SafenetEmptyResponse>> DnsPostAsync(string longName) =>
			EmptyResponseAsync(http.PostAsync($"/dns/{longName}", new StringContent(string.Empty)));

		public Task<SafenetResponse<SafenetEmptyResponse>> DnsPostAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PostAsync("/dns", ToPayload(service)));

		public Task<SafenetResponse<SafenetEmptyResponse>> DnsPutAsync(SafenetDnsRegisterServiceRequest service) =>
			EmptyResponseAsync(http.PutAsync("/dns", ToPayload(service)));

		public Task<SafenetResponse<SafenetFileResponse>> NfsGetFileAsync(string rootPath, string filePath) {
			var request = http.GetAsync($"/nfs/file/{rootPath}/{WebUtility.UrlEncode(filePath)}");
			return MakeFileResponse(request);
		} 

		public Task<SafenetResponse<SafenetEmptyResponse>> NfsPostAsync(SafenetNfsCreateDirectoryRequest directory) =>
			EmptyResponseAsync(http.PostAsync($"/nfs/directory/{directory.RootPath}/{directory.DirectoryPath}", ToPayload(new {
				isPrivate = directory.IsPrivate,
				metadata = Convert.ToBase64String(Encoding.UTF8.GetBytes(directory.Metadata)),
			})));

		public Task<SafenetResponse<SafenetEmptyResponse>> NfsPostAsync(SafenetNfsPutFileRequest file) {
			var body = new ByteArrayContent(file.Bytes);
			body.Headers.ContentType = file.ContentType;
			body.Headers.Add("Metadata", Convert.ToBase64String(file.Metadata));
			return EmptyResponseAsync(http.PostAsync($"/nfs/file/{file.RootPath}/{file.FilePath}", body));
		}

		public Task<SafenetResponse<SafenetEmptyResponse>> NfsDeleteFileAsync(string rootPath, string filePath) =>
			EmptyResponseAsync(http.DeleteAsync($"/nfs/file/{rootPath}/{filePath}")); 

		async Task<SafenetResponse<SafenetFileResponse>> MakeFileResponse(Task<HttpResponseMessage> request)
		{
			var r = await request;
			var response = new SafenetResponse<SafenetFileResponse> {StatusCode = r.StatusCode};
			if (r.IsSuccessStatusCode)
				response.Result = new SafenetFileResponse
				{
					CreatedOn = DateTime.Parse(r.Headers.GetValues("created-on").Single()),
					ModifiedOn = DateTime.Parse(r.Content.Headers.GetValues("last-modified").Single()),
					ContentLength = r.Content.Headers.ContentLength,
					ContentType = r.Content.Headers.ContentType,
					ContentRange = r.Content.Headers.ContentRange,
					Metadata = Convert.FromBase64String(HeaderOrDefaul(r.Headers, "metadata", string.Empty)),
					Body = await r.Content.ReadAsStreamAsync()
				};
			else
				using (var jr = new JsonTextReader(new StreamReader(await r.Content.ReadAsStreamAsync())))
					response.Error = json.Deserialize<SafenetError>(jr);
			return response;
		}

		static string HeaderOrDefaul(HttpHeaders headers, string header, string defaultValue) {
			IEnumerable<string> found;
			if(headers.TryGetValues(header, out found))
				return found.Single();
			return defaultValue;
		}

		async Task<SafenetResponse<SafenetEmptyResponse>> EmptyResponseAsync(Task<HttpResponseMessage> request) {
			var r = await request;
			var response = new SafenetResponse<SafenetEmptyResponse> {StatusCode = r.StatusCode};
			if (!r.IsSuccessStatusCode)
				using (var jr = new JsonTextReader(new StreamReader(await r.Content.ReadAsStreamAsync())))
					response.Error = json.Deserialize<SafenetError>(jr);
			return response;
		}

		async Task<SafenetResponse<T>> MakeResponseAsync<T>(Task<HttpResponseMessage> request) {
			var r = await request;
			var response = new SafenetResponse<T> {StatusCode = r.StatusCode};
			using (var jr = new JsonTextReader(new StreamReader(await r.Content.ReadAsStreamAsync())))
				if (r.IsSuccessStatusCode)
					response.Result = json.Deserialize<T>(jr);
				else
					response.Error = json.Deserialize<SafenetError>(jr);
			return response;
		}
	
		HttpContent ToPayload(object obj) =>
			new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
	}
}