using System;
using System.Text;

namespace LibuvSharp
{
	public static class IUVStreamExtensions
	{
		public static void Read(this IUVStream stream, Encoding enc, Action<string> callback)
		{
			stream.Data += (data) => callback(enc.GetString(data.Array, data.Offset, data.Count));
		}

		public static void Write(this IUVStream stream, byte[] data, int index, int count)
		{
			stream.Write(data, index, count, null);
		}

		public static void Write(this IUVStream stream, byte[] data, int count, Action<bool> callback)
		{
			stream.Write(data, 0, count, callback);
		}
		public static void Write(this IUVStream stream, byte[] data, int count)
		{
			stream.Write(data, count, null);
		}

		public static void Write(this IUVStream stream, byte[] data, Action<bool> callback)
		{
			stream.Write(data, data.Length, callback);
		}
		public static void Write(this IUVStream stream, byte[] data)
		{
			stream.Write(data, null);
		}

		public static void Write(this IUVStream stream, ArraySegment<byte> data, Action<bool> callback)
		{
			stream.Write(data.Array, data.Offset, data.Count, callback);
		}
		public static void Write(this IUVStream stream, ArraySegment<byte> data)
		{
			stream.Write(data, null);
		}

		public static int Write(this IUVStream stream, Encoding enc, string text, Action<bool> callback)
		{
			var bytes = enc.GetBytes(text);
			stream.Write(bytes, callback);
			return bytes.Length;
		}
		public static int Write(this IUVStream stream, string text, Action<bool> callback)
		{
			return stream.Write(Encoding.Default, text, callback);
		}
		public static int Write(this IUVStream stream, Encoding enc, string text)
		{
			return stream.Write(enc, text, null);
		}
		public static int Write(this IUVStream stream, string text)
		{
			return stream.Write(Encoding.Default, text);
		}

		public static void Shutdown(this IUVStream stream)
		{
			stream.Shutdown(null);
		}

		public static void End(this IUVStream stream, byte[] data, int index, int count, Action callback)
		{
			stream.Write(data, index, count);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, byte[] data, int index, int count)
		{
			stream.End(data, index, count, null);
		}

		public static void End(this IUVStream stream, byte[] data, int count, Action callback)
		{
			stream.Write(data, count);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, byte[] data, int count)
		{
			stream.Write(data, count, null);
		}

		public static void End(this IUVStream stream, byte[] data, Action callback)
		{
			stream.Write(data);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, byte[] data)
		{
			stream.Write(data, null);
		}

		public static void End(this IUVStream stream, ArraySegment<byte> data, Action callback)
		{
			stream.Write(data);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, ArraySegment<byte> data)
		{
			stream.End(data, null);
		}

		public static int End(this IUVStream stream, Encoding encoding, string text, Action callback)
		{
			int size = stream.Write(encoding, text);
			stream.Shutdown(callback);
			return size;
		}
		public static int End(this IUVStream stream, string text, Action callback)
		{
			return stream.End(Encoding.Default, text, callback);
		}
		public static int End(this IUVStream stream, Encoding encoding, string text)
		{
			return stream.End(encoding, text, null);
		}
		public static int End(this IUVStream stream, string text)
		{
			return stream.End(Encoding.Default, text);
		}
	}
}

