using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVTimerExtensions
	{
		public static Task StartAsync(this UVTimer timer, ulong timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				timer.Start(timeout, () => {
					tcs.SetResult(null);
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}

		public static Task StartAsync(this UVTimer timer, TimeSpan timeout)
		{
			return timer.StartAsync((ulong)timeout.TotalMilliseconds);
		}
	}
}
