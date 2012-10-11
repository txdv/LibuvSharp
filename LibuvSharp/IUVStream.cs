using System;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream
	{
		Loop Loop { get; }

		event Action Complete;
		event Action<Exception> Error;

		void Resume();
		void Pause();

		void Read(Action<ByteBuffer> callback);

		void Write(byte[] data, int index, int count, Action<bool> callback);

		void Shutdown(Action callback);
	}
}
