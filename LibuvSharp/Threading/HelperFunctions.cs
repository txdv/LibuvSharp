using System;
using System.Threading.Tasks;

namespace LibuvSharp
{
	class HelperFunctions
	{
		public static Action<Exception, T> Finish<T>(TaskCompletionSource<T> tcs, Action callback)
		{
			bool finished = false;

			return (Exception exception, T value) => {
				if (finished) {
					return;
				}

				finished = true;

				if (callback != null) {
					callback();
				}

				if (exception != null) {
					tcs.SetException(exception);
				} else {
					tcs.SetResult(value);
				}
			};
		}

		public static Task Wrap<T>(T arg1, Action<T, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, (ex) => {
					if (ex != null) {
						tcs.SetException(ex);
					} else {
						tcs.SetResult(null);
					}
				});
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task Wrap<T1, T2>(T1 arg1, T2 arg2, Action<T1, T2, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, arg2, (ex) => {
					if (ex != null) {
						tcs.SetException(ex);
					} else {
						tcs.SetResult(null);
					}
				});
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
	}
}

