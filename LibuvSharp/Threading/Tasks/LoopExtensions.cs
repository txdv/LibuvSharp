using System;
using System.Threading.Tasks;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class LoopExtensions
	{
		public static Task Async(this Loop loop, Action action)
		{
			var tcs = new TaskCompletionSource<object>();
			Exception exception = null;
			loop.QueueUserWorkItem(() => {
				try {
					action();
				} catch (Exception e) {
					exception = e;
				}
			}, () => {
				if (exception == null) {
					tcs.SetResult(null);
				} else {
					tcs.SetException(exception);
				}
			});
			return tcs.Task;
		}

		public static Task<T> FAsync<T>(this Loop loop, Func<T> action)
		{
			var tcs = new TaskCompletionSource<T>();
			Exception exception = null;
			T res = default(T);
			loop.QueueUserWorkItem(() => {
				try {
					res = action();
				} catch (Exception e) {
					exception = e;
				}
			}, () => {
				if (exception == null) {
					tcs.SetResult(res);
				} else {
					tcs.SetException(exception);
				}
			});
			return tcs.Task;
		}
	}
}
