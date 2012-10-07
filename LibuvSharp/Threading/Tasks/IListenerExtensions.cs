using System;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IListenerExtensions
	{
		public static Task<IUVStream> AcceptStreamAsync(this IListener listener)
		{
			return AcceptStreamAsync(listener, listener.DefaultBacklog);
		}

		public static Task<IUVStream> AcceptStreamAsync(this IListener listener, int backlog)
		{
			var tcs = new TaskCompletionSource<IUVStream>();
			try {
				listener.Listen(backlog, (stream) => {
					tcs.SetResult(stream);
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
	}
}

