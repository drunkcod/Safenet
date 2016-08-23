using System;
using System.Collections.Generic;
using System.Threading;

namespace MySafenet
{
	class ThreadPoolWorker
	{
		public void Post<T0>(Action<T0> action, T0 arg0) => Invoke(action, arg0);
		public void Post<T0,T1>(Action<T0,T1> action, T0 arg0, T1 arg1) => Invoke(action, arg0, arg1);

		static void Invoke(Delegate target, params object[] args) =>
			ThreadPool.QueueUserWorkItem(InvokeIt, new KeyValuePair<Delegate, object[]>(target, args));

		static readonly WaitCallback InvokeIt = obj => {
			var x = (KeyValuePair<Delegate,object[]>)obj;
			x.Key.DynamicInvoke(x.Value);
		};
	}
}