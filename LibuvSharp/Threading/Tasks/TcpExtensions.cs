using System;
using System.Net;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class TcpExtensions
	{
		public static Task ConnectAsync(this Tcp tcp, IPEndPoint ep)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				tcp.Connect(ep, (e) => {
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

		public static Task ConnectAsync(this Tcp tcp, IPAddress ipAddress, int port)
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

		public static Task ConnectAsync(this Tcp tcp, IPEndPoint ep, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				tcp.Connect(ep, timeout, (e) => {
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

		public static Task ConnectAsync(this Tcp tcp, IPAddress ipAddress, int port, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				tcp.Connect(ipAddress, port, timeout, (e) => {
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

		public static Task ConnectAsync(this Tcp tcp, string ipAddress, int port, TimeSpan timeout)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				tcp.Connect(ipAddress, port, timeout, (e) => {
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

	}
}

