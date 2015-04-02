using System;
using System.Text;

namespace LibuvSharp
{
	public static class IUVStreamExtensions
	{
		#region Read

		public static void Read(this IUVStream stream, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			stream.Read(new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Read(this IUVStream stream, byte[] data, int index, Action<Exception, int> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			stream.Read(data, index, data.Length - index, callback);
		}

		public static void Read(this IUVStream stream, byte[] data, Action<Exception, int> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			stream.Read(data, 0, data.Length, callback);
		}

		#endregion

		#region Write

		public static void Write(this IUVStream stream, byte[] data, int index, int count, Action<Exception> callback)
		{
			stream.Write(new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Write(this IUVStream stream, byte[] data, int index, int count)
		{
			stream.Write(data, index, count, null);
		}

		public static void Write(this IUVStream stream, byte[] data, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			stream.Write(data, 0, data.Length, callback);
		}
		public static void Write(this IUVStream stream, byte[] data)
		{
			stream.Write(data, null);
		}

		public static void Write(this IUVStream stream, ArraySegment<byte> data)
		{
			stream.Write(data, null);
		}

		#endregion

		#region Write string

		public static int Write(this IUVStream stream, Encoding enc, string text, Action<Exception> callback)
		{
			var bytes = enc.GetBytes(text);
			stream.Write(bytes, callback);
			return bytes.Length;
		}
		public static int Write(this IUVStream stream, string text, Action<Exception> callback)
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

		#endregion

		#region Shutdown

		public static void Shutdown(this IUVStream stream)
		{
			stream.Shutdown(null);
		}

		public static void Shutdown(this IUVStream stream, Action callback)
		{
			stream.Shutdown((_) => callback());
		}

		#endregion

		#region End

		public static void End(this IUVStream stream, byte[] data, int index, int count, Action<Exception> callback)
		{
			stream.Write(data, index, count);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, byte[] data, int index, int count)
		{
			stream.End(data, index, count, null);
		}

		public static void End(this IUVStream stream, byte[] data, Action<Exception> callback)
		{
			stream.Write(data);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, byte[] data)
		{
			stream.Write(data, null);
		}

		public static void End(this IUVStream stream, ArraySegment<byte> data, Action<Exception> callback)
		{
			stream.Write(data);
			stream.Shutdown(callback);
		}
		public static void End(this IUVStream stream, ArraySegment<byte> data)
		{
			stream.End(data, null);
		}

		public static int End(this IUVStream stream, Encoding encoding, string text, Action<Exception> callback)
		{
			int size = stream.Write(encoding, text);
			stream.Shutdown(callback);
			return size;
		}
		public static int End(this IUVStream stream, string text, Action<Exception> callback)
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

		#endregion
	}
}

