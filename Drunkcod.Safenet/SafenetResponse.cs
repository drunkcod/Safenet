using System.Net;

namespace Drunkcod.Safenet
{
	public class SafenetResponse<T>
	{
		public T Response;
		public SafenetError? Error;
		public readonly HttpStatusCode StatusCode;

		public SafenetResponse(HttpStatusCode status) {
			this.StatusCode = status;
		}
	}

	public struct SafenetEmptyResponse
	{
		public readonly SafenetError? Error;
		public readonly HttpStatusCode StatusCode;

		public SafenetEmptyResponse(HttpStatusCode status, SafenetError? error) {
			this.StatusCode = status;
			this.Error = error;
		}
	}

}