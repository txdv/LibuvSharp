using System;
using System.Collections.Generic;

namespace LibuvSharp
{
	public static class IPCExtensions
	{
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
	}
}

