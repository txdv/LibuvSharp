using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class TaskExtensions
	{
		public static Task ConnectAsync(this Tcp tcp, string ipAddress, int port)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				tcp.Connect(ipAddress, port, (e) => {
					if (e == null) {
						tcs.SetResult(null);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}

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
