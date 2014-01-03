using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

static class AsyncExtensions
{
	public static async Task<string> ReadStringAsync(this IUVStream stream)
	{
		return await ReadStringAsync(stream, Encoding.Default);
	}

	public static async Task<string> ReadStringAsync(this IUVStream stream, Encoding encoding)
	{
		if (encoding == null) {
			throw new ArgumentException("encoding");
		}
		var buffer = await stream.ReadAsync();
		if (!buffer.HasValue) {
			return null;
		}
		var b = buffer.Value;
		return encoding.GetString(b.Array, b.Offset, b.Count);
	}

	public static Task QueueUserWorkItemAsync(this Loop loop, Action work)
	{
		var tcs = new TaskCompletionSource<object>();
		try {
			loop.QueueUserWorkItem(work, () => {
				tcs.SetResult(null);
			});
		} catch (Exception e) {
			tcs.SetException(e);
		}
		return tcs.Task;
	}
}
