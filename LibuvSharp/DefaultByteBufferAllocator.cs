using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class DefaultByteBufferAllocator : AbstractByteBufferAllocator
	{
		public byte[] Buffer { get; protected set; }
		public int Size { get; protected set; }
		GCHandle GCHandle { get; set; }

		public override IntPtr Alloc(int size)
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

		public override void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}
			Free();
		}

		void Free()
		{
			if (GCHandle.IsAllocated) {
				GCHandle.Free();
			}
			Buffer = null;
			Size = 0;
		}

		public override ArraySegment<byte> Retrieve(int size)
		{
			byte[] ret = new byte[size];
			Array.Copy(Buffer, 0, ret, 0, size);
			return new ArraySegment<byte>(ret);
		}
	}
}

