using System;
using System.Text;

namespace LibuvSharp
{
	public interface IUVStream
	{
		Loop Loop { get; }

		event Action EndOfStream;
		event Action<Exception> Error;

		void Resume();
		void Pause();

		void Read(Action<ByteBuffer> callback);
		void Read(Encoding enc, Action<string> callback);

		void Write(byte[] data, int index, int count, Action<bool> callback);
		void Write(byte[] data, int index, int count);

		void Write(byte[] data, int count, Action<bool> callback);
		void Write(byte[] data, int count);

		void Write(byte[] data, Action<bool> callback);
		void Write(byte[] data);

		void Write(Encoding enc, string text, Action<bool> callback);
		void Write(Encoding enc, string text);

		void Shutdown(Action callback);
		void Shutdown();
	}
}
