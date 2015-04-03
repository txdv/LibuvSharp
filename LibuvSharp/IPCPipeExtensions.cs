using System;
using System.Text;
using System.Collections.Generic;

namespace LibuvSharp
{
	public static class IPCPipeExtensions
	{
		#region Write

		public static void Write(this IPCPipe pipe, Handle handle, byte[] data, int index, int count, Action<Exception> callback)
		{
			pipe.Write(handle, new ArraySegment<byte>(data, index, count), callback);
		}
		public static void Write(this IPCPipe pipe, Handle handle, byte[] data, int index, int count)
		{
			pipe.Write(handle, data, index, count, null);
		}
		public static void Write(this IPCPipe pipe, Handle handle, byte[] data, int index, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			pipe.Write(handle, data, index, data.Length - index, callback);
		}
		public static void Write(this IPCPipe pipe, Handle handle, byte[] data, int index)
		{
			pipe.Write(handle, data, index, null);
		}
		public static void Write(this IPCPipe pipe, Handle handle, byte[] data, Action<Exception> callback)
		{
			pipe.Write(handle, data, 0, callback);
		}
		public static void Write(this IPCPipe pipe, Handle handle, byte[] data)
		{
			pipe.Write(handle, data, null);
		}

		#endregion

		#region Write string

		public static int Write(this IPCPipe pipe, Handle handle, Encoding enc, string text, Action<Exception> callback)
		{
			var bytes = enc.GetBytes(text);
			pipe.Write(handle, bytes, callback);
			return bytes.Length;
		}
		public static int Write(this IPCPipe pipe, Handle handle, string text, Action<Exception> callback)
		{
			return pipe.Write(handle, Encoding.Default, text, callback);
		}
		public static int Write(this IPCPipe pipe, Handle handle, Encoding enc, string text)
		{
			return pipe.Write(enc, text, null);
		}
		public static int Write(this IPCPipe pipe, Handle handle, string text)
		{
			return pipe.Write(Encoding.Default, text);
		}

		#endregion

	}
}

