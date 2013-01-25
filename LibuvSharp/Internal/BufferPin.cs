using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	class BufferPin : IDisposable
	{
		public byte[] Buffer { get; protected set; }
		public GCHandle GCHandle { get; protected set; }

		public BufferPin(int count)
			: this(new byte[count])
		{
		}

		public BufferPin(byte[] buffer)
			: this(buffer, 0, buffer.LongLength)
		{
		}

		public BufferPin(byte[] buffer, int offset, int count)
			: this(buffer, (IntPtr)offset, (IntPtr)count)
		{
		}

		public BufferPin(byte[] buffer, long offset, long count)
			: this(buffer, (IntPtr)offset, (IntPtr)count)
		{
		}

		public BufferPin(byte[] buffer, IntPtr offset, IntPtr count)
		{
			Buffer = buffer;
			Offset = (IntPtr)offset;
			Count = (IntPtr)count;
			Alloc();
		}

		public void Alloc()
		{
			GCHandle = GCHandle.Alloc(Buffer, GCHandleType.Pinned);
			Pointer = GCHandle.AddrOfPinnedObject();
		}

		~BufferPin()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (GCHandle.IsAllocated) {
				GCHandle.Free();
			}
			Pointer = IntPtr.Zero;
		}

		public IntPtr Pointer { get; protected set; }

		public IntPtr Offset { get; protected set; }
		public IntPtr Count { get; protected set; }

		public IntPtr Start {
			get {
				return At(Offset);
			}
		}
		public IntPtr End {
			get {
				return At(Offset.ToInt64() + Count.ToInt64());
			}
		}

		public IntPtr At(int offset)
		{
			return At((IntPtr)offset);
		}
		public IntPtr At(long offset)
		{
			return At((IntPtr)offset);
		}
		public IntPtr At(IntPtr offset)
		{
			return (IntPtr)(Pointer.ToInt64() + offset.ToInt64());
		}

		public IntPtr GetOffset(IntPtr ptr)
		{
			return (IntPtr)(ptr.ToInt64() - Start.ToInt64());
		}

		public bool Fits(IntPtr ptr)
		{
			return (Start.ToInt64() <= ptr.ToInt64()) &&
				ptr.ToInt64() <= End.ToInt64();
		}
	}
}
