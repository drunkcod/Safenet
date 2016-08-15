using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Drunkcod.Safenet
{
	class JsonContent : HttpContent
	{
		static readonly MediaTypeHeaderValue ApplicationJson = MediaTypeHeaderValue.Parse("application/json");

		readonly JsonSerializer json;
		readonly object value;

		public JsonContent(JsonSerializer json, object value) {
			this.json = json;
			this.value = value;
			Headers.ContentType = ApplicationJson;
		}

		protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) {
			using(var writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
				json.Serialize(writer, value);
			return Task.FromResult((object)null);
		}

		protected override bool TryComputeLength(out long length) {
			length = 0;
			return false;
		}
	}
}