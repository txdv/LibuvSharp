using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract partial class HandleBase : Handle
	{
		internal HandleBase(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
		}

		internal HandleBase(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal HandleBase(Loop loop, HandleType type)
			: this(loop, Handle.Size(type))
		{
		}

		internal HandleBase(Loop loop, HandleType type, Func<IntPtr, IntPtr, int> constructor)
			: this(loop, type)
		{
			Construct(constructor);
		}

		internal struct ReadRequest
		{
			public ArraySegment<byte> buf;
			public Action<Exception, int> cb;
			public Action<Exception, UdpReceiveMessage> ucb;
			public int read;
			public GCHandle gchandle;
		}

		internal Queue<ReadRequest> readRequests = new Queue<ReadRequest>();

		internal static Handle.alloc_callback_unix alloc_unix;
		internal static Handle.alloc_callback_win alloc_win;

		static HandleBase()
		{
			alloc_unix = AllocUnix;
			alloc_win = AllocWin;
		}

		static void AllocUnix(IntPtr handlePointer, int size, out UnixBufferStruct buf)
		{
			IntPtr ptr;
			var handle = FromIntPtr<HandleBase>(handlePointer);
			size = handle.Alloc(size, out ptr);
			buf = new UnixBufferStruct(ptr, size);
		}

		static void AllocWin(IntPtr handlePointer, int size, out WindowsBufferStruct buf)
		{
			IntPtr ptr;
			var handle = FromIntPtr<HandleBase>(handlePointer);
			size = handle.Alloc(size, out ptr);
			buf = new WindowsBufferStruct(ptr, size);
		}

		int Alloc(int size, out IntPtr pointer)
		{
			var req = readRequests.Peek();
			req.gchandle = GCHandle.Alloc(req.buf.Array, GCHandleType.Pinned);
			pointer = req.gchandle.AddrOfPinnedObject() + req.buf.Offset;
			return req.buf.Count;
		}
	}
}

