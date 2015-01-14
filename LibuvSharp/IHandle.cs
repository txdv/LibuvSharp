using System;

namespace LibuvSharp
{
	public interface IHandle
	{
		void Unref();
		void Close(Action callback);
	}
}

