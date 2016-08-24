using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	public static class TaskExtensions
	{
		public static void AwaitResult(this Task self) => 
			self.ConfigureAwait(false).GetAwaiter().GetResult();

		public static T AwaitResult<T>(this Task<T> self) => 
			self.ConfigureAwait(false).GetAwaiter().GetResult();

		public static SafenetResponse AwaitResponse(this Task<SafenetResponse> self) =>
			self.AwaitResult();

		public static T AwaitResponse<T>(this Task<SafenetResponse<T>> self) =>
			self.AwaitResult().Response;
	}
}