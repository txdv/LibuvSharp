using System;

namespace LibuvSharp
{
	public interface ITryWrite<TData>
	{
		int TryWrite(TData data);
	}
}

