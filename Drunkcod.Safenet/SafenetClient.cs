using System;
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
	
		public SafenetClient(string apiRoot = "http://localhost:8100")
		{
			this.http = new HttpClient {
				BaseAddress = new Uri(apiRoot),
			};
		}

		public void SetToken(string token)
		{
			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		public async Task<SafenetResponse<SafenetDirectoryResponse>> DnsGetAsync(string service, string longName)
		{
			var r = await http.GetAsync($"/dns/{service}/{longName}");
			var response = new SafenetResponse<SafenetDirectoryResponse>();
			response.StatusCode = r.StatusCode;
			if (r.IsSuccessStatusCode)
				response.Result = (SafenetDirectoryResponse)JsonConvert.DeserializeObject((await r.Content.ReadAsStringAsync()), typeof(SafenetDirectoryResponse));
			else
				response.Error = (SafenetError)JsonConvert.DeserializeObject((await r.Content.ReadAsStringAsync()), typeof(SafenetError));
			return response;
		}
	
		HttpContent ToPayload(object obj) =>
			new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
	}
}