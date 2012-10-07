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

		public static Task ShutdownAsync(this UVStream stream)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				stream.Shutdown(() => {
					tcs.SetResult(null);
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}

		public static Task<ByteBuffer> ReadAsync(this UVStream stream)
		{
			var tcs = new TaskCompletionSource<ByteBuffer>();
			Action<Exception> error = (e) => tcs.SetException(e);
			Action end = () => tcs.SetResult(null);
			try {
				stream.Error += error;
				stream.EndOfStream += end;
				stream.Read((buffer) => {
					tcs.SetResult(buffer);
				});
				stream.Resume();
			} catch (ArgumentException) {
				tcs.SetResult(null);
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task.ContinueWith((bb) => {
				stream.Pause();
				stream.Error -= error;
				stream.EndOfStream -= end;
				return bb.Result;
			}, stream.Loop.Scheduler);
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
