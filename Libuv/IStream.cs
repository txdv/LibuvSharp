using System;
using System.Text;

namespace Libuv
{
	public interface IStream
	{
		void Resume();
		void Pause();

		void Read(Action<byte[]> callback);
		void Read(Encoding enc, Action<string> callback);

		void Write(byte[] data, int length, Action<int> callback);
		void Write(byte[] data, Action<int> callback);
		void Write(byte[] data, Action callback);
		void Write(Encoding enc, string text, Action<int> callback);
		void Write(Encoding enc, string text, Action callback);
	}
}
