using System;
using System.Net;

namespace LibuvSharp
{
	public interface ITrySend<TEndPoint>
	{
		int TrySend(TEndPoint ipEndPoint, ArraySegment<byte> data);
	}
}

