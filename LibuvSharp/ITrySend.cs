using System;

namespace LibuvSharp
{
	public interface ITrySend<TMessage>
	{
		int TrySend(TMessage message);
	}
}

