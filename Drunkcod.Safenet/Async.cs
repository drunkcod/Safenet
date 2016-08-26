using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	static class Async
	{
		class ActionTaskState<T>
		{
			readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
			Func<T> action;

			public Task<T> Begin(Func<T> action) {
				this.action = action;
				action.BeginInvoke(Callback, this);
				return tcs.Task;
			}

			static readonly AsyncCallback Callback = ar => {
				var state = (ActionTaskState<T>)ar.AsyncState;

				try { state.tcs.SetResult(state.action.EndInvoke(ar)); }
				catch(Exception ex) { state.tcs.SetException(ex); }
			};
		} 

		public static Task ToAsync(Action action) =>
			new ActionTaskState<bool>().Begin(() => { action(); return true; });

		public static Task<T> ToAsync<T>(Func<T> action) =>
			new ActionTaskState<T>().Begin(action);

		public static Task<bool> WaitAsync(this WaitHandle self) =>
			ToAsync(self.WaitOne);
	}
}