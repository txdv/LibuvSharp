using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class ByteBuffer : IDisposable
	{
		public byte[] Buffer { get; protected set; }
		public int Size { get; protected set; }
		public Tcp.alloc_callback_unix AllocCallbackUnix { get; protected set; }
		public Tcp.alloc_callback_win AllocCallbackWin { get; protected set; }
		GCHandle GCHandle { get; set; }

		public ByteBuffer()
		{
			AllocCallbackUnix = AllocUnix;
			AllocCallbackWin = AllocWin;
		}

		~ByteBuffer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}
			Free();
		}

		IntPtr Alloc(int size)
		{
			if (Buffer == null) {
				Buffer = new byte[size];
				Size = size;
				GCHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			} else if (Size < size) {
				Free();
				Buffer = new byte[size];
				Size = size;
				GCHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			}
			return GCHandle.AddrOfPinnedObject();
		}

		UnixBufferStruct AllocUnix(IntPtr data, int size)
		{
			return new UnixBufferStruct(Alloc(size), size);
		}

		WindowsBufferStruct AllocWin(IntPtr data, int size)
		{
			return new WindowsBufferStruct(Alloc(size), size);
		}

		void Free()
		{
			if (GCHandle.IsAllocated) {
				GCHandle.Free();
			}
			Buffer = null;
			Size = 0;
		}

		public byte[] Get(int size)
		{
			byte[] ret = new byte[size];
			Array.Copy(Buffer, 0, ret, 0, size);
			return ret;
		}
	}
}

