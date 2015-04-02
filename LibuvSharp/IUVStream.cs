using System;
using System.Collections.Generic;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream
	{
		Loop Loop { get; }

		bool Readable { get; }
		void Read(ArraySegment<byte> buffer, Action<Exception, int> callback);

		bool Writeable { get; }
		event Action Drain;
		long WriteQueueSize { get; }
		void Write(ArraySegment<byte> buffer, Action<Exception> callback);
		void Shutdown(Action<Exception> callback);
	}
}
