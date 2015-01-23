using System;
using System.Collections.Generic;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream<TMessage>
	{
		Loop Loop { get; }
		event Action<Exception> Error;

		bool Readable { get; }
		event Action Complete;
		event Action<TMessage> Data;
		void Resume();
		void Pause();

		bool Writeable { get; }
		event Action Drain;
		long WriteQueueSize { get; }
		void Write(TMessage data, Action<Exception> callback);
		void Shutdown(Action<Exception> callback);
	}
}
