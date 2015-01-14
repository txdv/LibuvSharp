using System;

namespace LibuvSharp
{
	public interface IHandle
	{
		void Close(Action callback);
	}

}

