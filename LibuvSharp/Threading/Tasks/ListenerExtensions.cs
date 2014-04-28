using System;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class ListenerExtensions
	{
		public static Task<T> AcceptAsync<T>(this Listener<T> listener) where T : class, IUVStream
		{
			var tcs = new TaskCompletionSource<T>();

			try {
				tcs.SetResult(listener.Accept());
			} catch (UVException ex) {
				if (ex.Code != 4) { // EAGAIN
					tcs.SetException(ex);
				}
			}

			if (tcs.Task.IsCompleted) {
				return tcs.Task;
			}

			Action<Exception, T> finish = null;

			Action connectioncb = () => {
				try {
					finish(null, listener.Accept());
				} catch (Exception ex) {
					finish(ex, null);
				}
			};

			finish = HelperFunctions.Finish(tcs, () => {
				listener.Connection -= connectioncb;
			});

			listener.Connection += connectioncb;

			return tcs.Task;
		}
	}
}

