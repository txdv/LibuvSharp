using System;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream
	{
		Loop Loop { get; }
		event Action<Exception> Error;

		bool Readable { get; }
		event Action Complete;
		void Read(Action<ByteBuffer> callback);
		void Resume();
		void Pause();

		bool Writeable { get; }
		void Write(byte[] data, int index, int count, Action<bool> callback);
		void Shutdown(Action callback);
	}
}
