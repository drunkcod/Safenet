using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace Drunkcod.Safenet.Simulator.Controllers
{
	public class LauncherApiController : ApiController
	{
		readonly HashSet<string> knownTokens;

		public LauncherApiController(HashSet<string> knownTokens)
		{
			this.knownTokens = knownTokens;
		}

		[HttpGet, Route("auth")]
		public HttpResponseMessage AuthGet() {
			if (IsAuthorized(Request.Headers.Authorization))
				return new HttpResponseMessage(HttpStatusCode.OK);
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		[HttpPost, Route("auth")]
		public object AuthPost() {
			var token = Guid.NewGuid().ToString();
			knownTokens.Add(token);
			return new { token };
		}

		[HttpDelete, Route("auth")]
		public HttpResponseMessage AuthDelete()
		{
			var auth = Request.Headers.Authorization;
			if (IsAuthorized(auth)) {
				knownTokens.Remove(auth.Parameter);
				return new HttpResponseMessage(HttpStatusCode.OK);
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		private bool IsAuthorized(AuthenticationHeaderValue auth)
		{
			return auth != null && auth.Scheme == "Bearer" && knownTokens.Contains(auth.Parameter);
		}
	}
}