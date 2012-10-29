using System;
using System.Collections.Generic;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream
	{
		Loop Loop { get; }
		event Action<Exception> Error;

		bool Readable { get; }
		event Action Complete;
		event Action<ArraySegment<byte>> Data;
		void Resume();
		void Pause();

		bool Writeable { get; }
		event Action Drain;
		long WriteQueueSize { get; }
		void Write(byte[] data, int index, int count, Action<bool> callback);
		void Shutdown(Action callback);
	}
}
