using System;

namespace LibuvSharp
{
	public interface IHandle
	{
		void Ref();
		void Unref();
		bool IsClosed { get; }
		void Close(Action callback);
	}
}

