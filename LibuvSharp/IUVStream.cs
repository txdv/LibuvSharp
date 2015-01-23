using System;
using System.Collections.Generic;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream<TData>
	{
		Loop Loop { get; }
		event Action<Exception> Error;

		bool Readable { get; }
		event Action Complete;
		event Action<TData> Data;
		void Resume();
		void Pause();

		bool Writeable { get; }
		event Action Drain;
		long WriteQueueSize { get; }
		void Write(TData data, Action<Exception> callback);
		void Shutdown(Action<Exception> callback);
	}
}
