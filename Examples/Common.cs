using System;
using System.Net;
using System.Net.Sockets;
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
		return encoding.GetString(buffer.Value);
	}
}

public static class EncodingExtensions
{
	public static string GetString(this Encoding encoding, ArraySegment<byte> segment)
	{
		return encoding.GetString(segment.Array, segment.Offset, segment.Count);
	}

	public static string GetString(this Encoding encoding, ArraySegment<byte>? segment)
	{
		if (!segment.HasValue) {
			return null;
		} else {
			var value = segment.Value;
			return encoding.GetString(value.Array, value.Offset, value.Count);
		}
	}
}

static class TcpClientExtensions
{
	public static async Task ConnectAsync(this TcpClient client, IPEndPoint ipEndPoint)
	{
		await client.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
	}
}
