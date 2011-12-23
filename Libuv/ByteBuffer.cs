using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	internal class ByteBuffer : IDisposable
	{
		public IntPtr Buffer { get; protected set; }
		public int Size { get; protected set; }

		~ByteBuffer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Dispose(bool disposing)
		{
			Free();
			GC.SuppressFinalize(this);
		}

		public IntPtr Alloc(int size)
		{
			if (Buffer == IntPtr.Zero) {
				Buffer = Marshal.AllocHGlobal(size);
				Size = size;
			} else if (Size < size) {
				Free();
				Buffer = Marshal.AllocHGlobal(size);
				Size = size;
			}
			return Buffer;
		}

		public UnixBufferStruct Alloc(IntPtr data, int size)
		{
			return new UnixBufferStruct(Alloc(size), size);
		}

		void Free()
		{
			Marshal.FreeHGlobal(Buffer);
			Buffer = IntPtr.Zero;
			Size = 0;
		}

		public byte[] Get(int size)
		{
			byte[] data = new byte[size];
			Marshal.Copy(Buffer, data, 0, size);
			return data;
		}
	}
}

