using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Drunkcod.Safenet
{
	public static class KeyValuePair
	{
		public static KeyValuePair<TKey,TValue> From<TKey,TValue>(TKey key, TValue value) => new KeyValuePair<TKey,TValue>(key, value); 
	}

	public static class Async
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

		public static IEnumerable<TResult> GetResults<TSource,TResult>(this ICollection<TSource> sources, Func<TSource,Task<TResult>> selector) {
			var r = new Task<TResult>[sources.Count];
			var n = 0;
			foreach(var item in sources)
				r[n++] = selector(item);

			return GetResults(r);
		}

		public static IEnumerable<T> GetResults<T>(this IEnumerable<Task<T>> sources) =>
			GetResults(sources.ToArray());

		static IEnumerable<T> GetResults<T>(Task<T>[] r) {
			while(r.Length != 0) {
				var done = Task.WaitAny(r);
				yield return r[done].AwaitResult();
				var last = r.Length - 1;
				r[done] = r[last];
				Array.Resize(ref r, last);
			}
		} 
	}
}