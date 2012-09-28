using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class AbstractByteBufferAllocator : IDisposable
	{
		internal Handle.alloc_callback_unix AllocCallbackUnix { get; set; }
		internal Handle.alloc_callback_win AllocCallbackWin { get; set; }

		public AbstractByteBufferAllocator()
		{
			AllocCallbackUnix = AllocUnix;
			AllocCallbackWin = AllocWin;
		}

		~AbstractByteBufferAllocator()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		UnixBufferStruct AllocUnix(IntPtr data, int size)
		{
			return new UnixBufferStruct(Alloc(size), size);
		}

		WindowsBufferStruct AllocWin(IntPtr data, int size)
		{
			return new WindowsBufferStruct(Alloc(size), size);
		}

		public abstract IntPtr Alloc(int size);
		public abstract void Dispose(bool disposing);
		public abstract ByteBuffer Retrieve(int size);
	}
}

