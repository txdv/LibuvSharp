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

			Action<Exception, ArraySegment<byte>?> finish = null;

			Action<Exception> error = (e) => finish(e, null);
			Action<ArraySegment<byte>> data = (val) => finish(null, val);
			Action end = () => finish(null, null);

			finish = HelperFunctions.Finish(tcs, () => {
				stream.Pause();
				stream.Error -= error;
				stream.Complete -= end;
				stream.Data -= data;
			});

			try {
				stream.Error += error;
				stream.Complete += end;
				stream.Data += data;
				stream.Resume();
			} catch (Exception e) {
				finish(e, null);
			}

			return tcs.Task;
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

