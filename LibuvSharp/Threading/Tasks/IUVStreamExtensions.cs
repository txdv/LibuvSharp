using System;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IUVStreamExtensions
	{
		public static Task<ArraySegment<byte>?> ReadAsync(this IUVStream stream)
		{
			var tcs = new TaskCompletionSource<ArraySegment<byte>?>();
			Action<Exception> error = (e) => tcs.SetException(e);
			Action<ArraySegment<byte>> data = (buffer) => tcs.SetResult(buffer);
			Action end = () => tcs.SetResult(null);
			try {
				stream.Error += error;
				stream.Complete += end;
				stream.Data += data;
				stream.Resume();
			} catch (ArgumentException) {
				tcs.SetResult(null);
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task.ContinueWith((bb) => {
				stream.Pause();
				stream.Error -= error;
				stream.Complete -= end;
				stream.Data -= data;
				return bb.Result;
			}, stream.Loop.Scheduler);
		}

		public static Task WriteAsync(this IUVStream stream, byte[] data, int index, int count)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				stream.Write(data, index, count, (_) => {
					tcs.SetResult(null);
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}

		public static Task WriteAsync(this IUVStream stream, byte[] data, int index)
		{
			return WriteAsync(stream, data, index, data.Length - index);
		}

		public static Task WriteAsync(this IUVStream stream, byte[] data)
		{
			return WriteAsync(stream, data, 0, data.Length);
		}

		public static Task ShutdownAsync(this IUVStream stream)
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
	}
}

