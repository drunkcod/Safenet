using System;
using System.Threading;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	static class Async
	{
		public static Task<T> ToAsync<T>(Func<T> action) {
			var tcs = new TaskCompletionSource<T>();
			action.BeginInvoke(ar => {
				tcs.SetResult(((Func<T>)ar.AsyncState).EndInvoke(ar));
			}, action);
			return tcs.Task;
		}

		public static Task<bool> WaitAsync(this WaitHandle self) =>
			ToAsync(self.WaitOne);
	}
}