using System;

namespace LibuvSharp
{
	public interface IRemoteAddress<T>
	{
		T RemoteAddress { get; }
	}
}

