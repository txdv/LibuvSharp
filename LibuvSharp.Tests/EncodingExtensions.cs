using System;
using System.Text;

namespace LibuvSharp.Tests
{
	public static class EncodingExtensions
	{
		public static string GetString(this Encoding encoding, ArraySegment<byte> segment)
		{
			return encoding.GetString(segment.Array, segment.Offset, segment.Count);
		}

		public static string GetString(this Encoding encoding, ArraySegment<byte>? segment)
		{
			if (segment.HasValue) {
				var value = segment.Value;
				return encoding.GetString(value.Array, value.Offset, value.Count);
			} else {
				return null;
			}
		}
	}
}

