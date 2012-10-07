using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class TaskExtensions
	{
		public static Task WaitAsync(this LibuvSharp.Timer timer, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				timer.Start(timeout, TimeSpan.Zero, () => {
					tcs.SetResult(null);
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
	}
}
