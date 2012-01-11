using System;
using System.Text;

namespace Libuv
{
	public interface IStream
	{
		void Pause();

		event Action<byte[]> OnRead;
		void Read(Action<byte[]> callback);
		void Read(Encoding enc, Action<string> callback);
		void Resume();

		void Write(byte[] data, int length, Action<bool> callback);
		void Write(byte[] data, int length);

		void Write(byte[] data, Action<bool> callback);
		void Write(byte[] data);

		void Write(Encoding enc, string text, Action<bool> callback);
		void Write(Encoding enc, string text);
	}
}
