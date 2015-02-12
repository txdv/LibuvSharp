using System;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IUVStreamExtensions
	{
		public static Task<TMessage?> ReadAsync<TMessage>(this IUVStream<TMessage> stream) where TMessage : struct
		{
			var tcs = new TaskCompletionSource<TMessage?>();

			Action<Exception, TMessage?> finish = null;

			Action<Exception> error = (e) => finish(e, null);
			Action<TMessage> data = (val) => finish(null, val);
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

		public static Task WriteAsync(this IUVStream<ArraySegment<byte>> stream, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap(data, index, count, stream.Write);
		}

		public static Task WriteAsync(this IUVStream<ArraySegment<byte>> stream, byte[] data, int index)
		{
			return WriteAsync(stream, data, index, data.Length - index);
		}

		public static Task WriteAsync(this IUVStream<ArraySegment<byte>> stream, byte[] data)
		{
			return WriteAsync(stream, data, 0, data.Length);
		}

		public static Task<int> WriteAsync(this IUVStream<ArraySegment<byte>> stream, Encoding encoding, string text)
		{
			return HelperFunctions.Wrap<Encoding, string, int>(encoding, text, stream.Write);
		}

		public static Task<int> WriteAsync(this IUVStream<ArraySegment<byte>> stream, string text)
		{
			return stream.WriteAsync(Encoding.Default, text);
		}

		public static Task ShutdownAsync(this IUVStream<ArraySegment<byte>> stream)
		{
			return HelperFunctions.Wrap(stream.Shutdown);
		}
	}
}

