using System;
using System.Net;

namespace Drunkcod.Safenet
{
	public class SafenetErrorException : Exception
	{
		public readonly HttpStatusCode StatusCode;
		public readonly SafenetError Error;

		public SafenetErrorException(HttpStatusCode status, SafenetError error) {
			this.StatusCode = status;
			this.Error = error;
		}
	}
}