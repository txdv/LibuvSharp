using System;

namespace LibuvSharp
{
	public static class ArraySegmentExtensions
	{
		public static ArraySegment<T> Skip<T>(this ArraySegment<T> data, int count)
		{
			return new ArraySegment<T>(data.Array, data.Offset + count, data.Count - count);
		}
	}
}

