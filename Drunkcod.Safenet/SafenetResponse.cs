using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	public struct SafenetResponse
	{
		public readonly SafenetError? Error;
		public readonly HttpStatusCode StatusCode;

		public SafenetResponse(HttpStatusCode status, SafenetError? error) {
			this.StatusCode = status;
			this.Error = error;
		}

		public static async Task<SafenetResponse<T>> CreateAsync<T>(HttpResponseMessage r, Func<HttpResponseMessage,Task<T>> success, Func<HttpResponseMessage,Task<SafenetError>> error) =>
			r.IsSuccessStatusCode 
			? new SafenetResponse<T>(r.StatusCode, null, await success(r))
			: new SafenetResponse<T>(r.StatusCode, await error(r), default(T));
	}

	public class SafenetResponse<T>
	{
		public T Response;
		public SafenetError? Error;
		public readonly HttpStatusCode StatusCode;

		public SafenetResponse(HttpStatusCode status, SafenetError? error, T response) {
			this.StatusCode = status;
			this.Error = error;
			this.Response = response;
		}
	}
}