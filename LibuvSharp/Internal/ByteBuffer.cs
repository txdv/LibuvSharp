using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class ByteBuffer : IDisposable
	{
		public byte[] Buffer { get; protected set; }
		public int Size { get; protected set; }
		public Func<IntPtr, int, UnixBufferStruct> AllocCallback { get; protected set; }
		GCHandle GCHandle { get; set; }

		public ByteBuffer()
		{
			AllocCallback = Alloc;
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
			GCHandle.Free();
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

		UnixBufferStruct Alloc(IntPtr data, int size)
		{
			return new UnixBufferStruct(Alloc(size), size);
		}

		void Free()
		{
			GCHandle.Free();
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

