using System;
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

		public async Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path) {
			var request = http.GetAsync($"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}");
			return await MakeFileResponse(request);
		}

		public async Task<SafenetResponse<SafenetFileResponse>> DnsGetAsync(string service, string longName, string path, RangeHeaderValue range) {
			var rm = new HttpRequestMessage(HttpMethod.Get, $"/dns/{service}/{longName}/{WebUtility.UrlEncode(path)}");
			rm.Headers.Range = range;
			return await MakeFileResponse(http.SendAsync(rm));
		}

		private async Task<SafenetResponse<SafenetFileResponse>> MakeFileResponse(Task<HttpResponseMessage> request)
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
					Body = await r.Content.ReadAsStreamAsync()
				};
			else
				using (var jr = new JsonTextReader(new StreamReader(await r.Content.ReadAsStreamAsync())))
					response.Error = json.Deserialize<SafenetError>(jr);
			return response;
		}

		private async Task<SafenetResponse<T>> MakeResponseAsync<T>(Task<HttpResponseMessage> request) {
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