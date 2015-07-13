using System;
using System.Threading;
using System.Threading.Tasks;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class LoopExtensions
	{
		public static Task QueueUserWorkItemAsync(this Loop loop, Action action)
		{
			var tcs = new TaskCompletionSource<object>();
			#if TASK_STATUS
			HelperFunctions.SetStatus(tcs.Task, TaskStatus.Running);
			#endif
			Exception exception = null;
			try {
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
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task<T> QueueUserWorkItemAsync<T>(this Loop loop, Func<T> action)
		{
			var tcs = new TaskCompletionSource<T>();
			#if TASK_STATUS
			HelperFunctions.SetStatus(tcs.Task, TaskStatus.Running);
			#endif
			Exception exception = null;
			T res = default(T);
			try {
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
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
	}
}
