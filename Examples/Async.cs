using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

static class AsyncExtensions
{
	public static async Task<string> ReadStringAsync(this IUVStream<ArraySegment<byte>> stream)
	{
		return await ReadStringAsync(stream, Encoding.Default);
	}

	public static async Task<string> ReadStringAsync(this IUVStream<ArraySegment<byte>> stream, Encoding encoding)
	{
		if (encoding == null) {
			throw new ArgumentException("encoding");
		}
		var buffer = await stream.ReadStructAsync();
		if (!buffer.HasValue) {
			return null;
		}
		var b = buffer.Value;
		return encoding.GetString(b.Array, b.Offset, b.Count);
	}
}
