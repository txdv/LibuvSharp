using System;

namespace LibuvSharp
{
	public interface ITryWrite
	{
		int TryWrite(ArraySegment<byte> data);
	}
}

