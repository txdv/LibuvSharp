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

	}
}

