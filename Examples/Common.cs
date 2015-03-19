using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;
using System.Security.Cryptography;

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

public static class HexExtensions
{
	public static string ToHex(this byte[] bytes)
	{
		return String.Join(string.Empty, Array.ConvertAll(bytes, x => x.ToString("x2")));
	}

	public static string ToHex(this ArraySegment<byte> segment)
	{
		return String.Join(String.Empty, segment.Select((x) => x.ToString("x2")));
	}
}

public static class HashAlgorithmExtensions
{
	public static void TransformBlock(this HashAlgorithm hashAlgorithm, ArraySegment<byte> input, byte[] outputBuffer, int outputOffset)
	{
		hashAlgorithm.TransformBlock(input.Array, input.Offset, input.Count, outputBuffer, outputOffset);
	}

	public static void TransformBlock(this HashAlgorithm hashAlgorithm, ArraySegment<byte> input)
	{
		hashAlgorithm.TransformBlock(input, null, 0);
	}

	public static void TransformFinalBlock(this HashAlgorithm hashAlgorithm, ArraySegment<byte> input)
	{
		hashAlgorithm.TransformFinalBlock(input.Array, input.Offset, input.Count);
	}

	static byte[] emptyBuffer = new byte[0];

	public static void TransformFinalBlock(this HashAlgorithm hashAlgorithm)
	{
		hashAlgorithm.TransformFinalBlock(emptyBuffer, 0, 0);
	}
}
