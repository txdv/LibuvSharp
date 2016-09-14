using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class ByteBufferAllocatorBase : IDisposable
	{
		internal Handle.alloc_callback AllocCallback { get; set; }

		public ByteBufferAllocatorBase()
		{
			AllocCallback = Alloc;
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

		void Alloc(IntPtr data, int size, out uv_buf_t buf)
		{
			IntPtr ptr;
			size = Alloc(size, out ptr);
			buf = new uv_buf_t(ptr, size);
		}

		public abstract int Alloc(int size, out IntPtr pointer);
		public abstract void Dispose(bool disposing);
		public abstract ArraySegment<byte> Retrieve(int size);
	}
}

