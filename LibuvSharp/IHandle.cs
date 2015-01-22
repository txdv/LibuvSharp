using System;

namespace LibuvSharp
{
	public interface IHandle
	{
		void Ref();
		void Unref();
		bool HasRef { get; }
		bool IsClosed { get; }
		void Close(Action callback);
	}
}

