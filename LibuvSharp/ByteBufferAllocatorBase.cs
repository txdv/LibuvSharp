using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class ByteBufferAllocatorBase : IDisposable
	{
		internal Handle.alloc_callback_unix AllocCallbackUnix { get; set; }
		internal Handle.alloc_callback_win AllocCallbackWin { get; set; }

		public ByteBufferAllocatorBase()
		{
			AllocCallbackUnix = AllocUnix;
			AllocCallbackWin = AllocWin;
		}

		~ByteBufferAllocatorBase()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void AllocUnix(IntPtr data, int size, out UnixBufferStruct buf)
		{
			IntPtr ptr;
			size = Alloc(size, out ptr);
			buf = new UnixBufferStruct(ptr, size);
		}

		void AllocWin(IntPtr data, int size, out WindowsBufferStruct buf)
		{
			IntPtr ptr;
			size = Alloc(size, out ptr);
			buf = new WindowsBufferStruct(ptr, size);
		}

		public abstract int Alloc(int size, out IntPtr pointer);
		public abstract void Dispose(bool disposing);
		public abstract ArraySegment<byte> Retrieve(int size);
	}
}

