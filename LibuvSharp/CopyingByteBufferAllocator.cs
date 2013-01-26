using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class CopyingByteBufferAllocator : ByteBufferAllocatorBase
	{
		BufferPin pin;

		public byte[] Buffer {
			get {
				return pin.Buffer;
			}
		}

		public override int Alloc(int size, out IntPtr ptr)
		{
			if (pin == null) {
				pin = new BufferPin(size);
			} else if (pin.Buffer.Length < size) {
				pin.Dispose();
				pin = new BufferPin(size);
			}
			ptr = pin.Start;
			return pin.Count.ToInt32();
		}

		public override void Dispose(bool disposing)
		{
			if (pin != null) {
				pin.Dispose();
			}
			pin = null;
		}

		public override ArraySegment<byte> Retrieve(int size)
		{
			byte[] ret = new byte[size];
			Array.Copy(Buffer, 0, ret, 0, size);
			return new ArraySegment<byte>(ret);
		}
	}
}

