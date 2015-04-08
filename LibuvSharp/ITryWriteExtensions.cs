using System;
using System.Text;

namespace LibuvSharp
{
	public static class ITryWriteExtensions
	{
		public static int TryWrite(this ITryWrite<ArraySegment<byte>> handle, byte[] data, int index, int count)
		{
			return handle.TryWrite(new ArraySegment<byte>(data, index, count));
		}

		public static int TryWrite(this ITryWrite<ArraySegment<byte>> handle, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			return handle.TryWrite(data, 0, data.Length);
		}

		public static int TryWrite(this ITryWrite<ArraySegment<byte>> handle, Encoding encoding, string text)
		{
			return handle.TryWrite(encoding.GetBytes(text));
		}

		public static int TryWrite(this ITryWrite<ArraySegment<byte>> handle, string text)
		{
			return handle.TryWrite(Encoding.Default, text);
		}
	}
}

