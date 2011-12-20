using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Libuv
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_connect_t
	{
		public UvRequestType type;
		public IntPtr data;
		/*
		#if !__MonoCS__
		NativeOverlapped overlapped;
		IntPtr queued_bytes;
		uv_err_t error;
		IntPtr next_req;
		#endif
		*/
		public IntPtr cb;
		public IntPtr handle;
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowsBufferStruct
	{
		internal WindowsBufferStruct(IntPtr @base, int length)
			: this(@base, (ulong)length)
		{
		}

		internal WindowsBufferStruct(IntPtr @base, ulong length)
		{
			this.@base = @base;
			this.length = length;
		}
		internal ulong length;
		internal IntPtr @base;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct UnixBufferStruct
	{
		internal UnixBufferStruct(IntPtr @base, int length)
			: this(@base, (IntPtr)length)
		{
		}

		internal UnixBufferStruct(IntPtr @base, IntPtr length)
		{
			this.@base = @base;
			this.length = length;
		}

		internal IntPtr @base;
		internal IntPtr length;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct sockaddr
	{
		public short sin_family;
		public ushort sin_port;
	}

	[StructLayout(LayoutKind.Sequential, Size=16)]
	internal struct sockaddr_in
	{
	}

	[StructLayout(LayoutKind.Sequential, Size=28)]
	internal struct sockaddr_in6
	{
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_req_t
	{
		public UvRequestType type;
		public IntPtr data;
	}

	internal struct req_gc_handles
	{
		public GCHandle data;
		public GCHandle callback;
		public bool cb;
	}

	internal enum UvType : int
	{
		Loop
	}

	internal enum UvHandleType : int
	{
		Unknown,
		Tcp,
		Udp,
		NamedPipe,
		TTY,
		File,
		Timer,
		Prepare,
		Check,
		Idle,
		Async,
		AresTask,
		AresEvent,
		Process,
		FSEvent
	};

	internal enum UvRequestType : int
	{
		Unknown,
		Connect,
		Accept,
		Read,
		Write,
		Shutdown,
		Wakeup,
		UdpSend,
		FileSystem,
		Work,
		GetAddrInfo,
		Private,
	}

	public static class UV
	{
		internal static bool isUnix = (System.Environment.OSVersion.Platform == PlatformID.Unix) || (System.Environment.OSVersion.Platform == PlatformID.MacOSX);
		internal static bool IsUnix { get { return isUnix; } }
		
		[DllImport("uv")]
		internal extern static sockaddr_in uv_ip4_addr(string ip, int port);

		[DllImport("uv")]
		internal extern static sockaddr_in6 uv_ip6_addr(string ip, int port);

		[DllImport("__Internal")]
		internal extern static ushort ntohs(ushort bytes);

		[DllImport("uv")]
		internal extern static int uv_ip4_name(IntPtr src, byte[] dst, IntPtr size);

		[DllImport("uv")]
		internal extern static int uv_ip6_name(IntPtr src, byte[] dst, IntPtr size);

		unsafe internal static IPEndPoint GetIPEndPoint(IntPtr sockaddr)
		{
			sockaddr *sa = (sockaddr *)sockaddr;
			byte[] addr;
			if (sa->sin_family == 10) {
				addr = new byte[64];
				uv_ip6_name(sockaddr, addr, (IntPtr)addr.Length);

			} else {
				addr = new byte[64];
				uv_ip4_name(sockaddr, addr, (IntPtr)addr.Length);
			}

			int i = 0;
			while (i < addr.Length && addr[i] != 0) {
				i++;
			}

			IPAddress ip = IPAddress.Parse(System.Text.Encoding.ASCII.GetString(addr, 0, i));

			return new IPEndPoint(ip, ntohs(sa->sin_port));
		}

		[DllImport("uv")]
		internal static extern int uv_sizeof(UvType type);
		[DllImport("uv")]

		internal static extern int uv_sizeof_handle(UvHandleType type);
		[DllImport("uv")]

		internal static extern int uv_sizeof_req(UvRequestType type);

		internal static int Sizeof(UvType type)
		{
			return uv_sizeof(type);
		}

		internal static int Sizeof(UvHandleType type)
		{
			return uv_sizeof_handle(type);
		}

		internal static int Sizeof(UvRequestType type)
		{
			return uv_sizeof_req(type);
		}

		internal static int RequestSizeof(UvRequestType type)
		{
			return uv_sizeof_req(type);
		}

		internal static void EnsureSuccess(int errorCode)
		{
			if (errorCode < 0) {
				throw new Exception(errorCode.ToString());
			}
		}

		unsafe static internal req_gc_handles *Create(byte[] data, Action<int> callback)
		{
			req_gc_handles *handles = (req_gc_handles *)UV.Alloc(sizeof(req_gc_handles));

			handles->data = GCHandle.Alloc(data, GCHandleType.Pinned);
			if (callback != null) {
				handles->callback = GCHandle.Alloc(callback, GCHandleType.Pinned);
				handles->cb = true;
			} else {
				handles->cb = false;
			}

			return handles;
		}

		unsafe static internal void Finish(IntPtr reqgc, int status)
		{
			req_gc_handles *handles = (req_gc_handles *)reqgc;
			handles->data.Free();
			if (handles->cb) {
				Action<int> cb = (Action<int>)handles->callback.Target;
				cb(status);
				handles->callback.Free();
			}
			UV.Free((IntPtr)handles);
		}

		#region Memory

		[DllImport("uv")]
		internal static extern long uv_get_free_memory();

		public static long FreeMemory {
			get {
				return uv_get_free_memory();
			}
		}

		[DllImport("uv")]
		internal static extern long uv_get_total_memory();

		public static long TotalMemory {
			get {
				return uv_get_total_memory();
			}
		}

		#endregion

		[DllImport("uv")]
		internal static extern ulong uv_hrtime();

		public static ulong HourTime {
			get {
				return uv_hrtime();
			}
		}
#if DEBUG
		static List<IntPtr> pointers = new List<IntPtr>();
#endif

		internal static IntPtr Alloc(int size)
		{
			IntPtr ptr = Marshal.AllocHGlobal(size);
#if DEBUG
			pointers.Add(ptr);
#endif
			return ptr;
		}

		internal static void Free(IntPtr ptr)
		{
			pointers.Remove(ptr);
			Marshal.FreeHGlobal(ptr);
		}

		unsafe internal static UnixBufferStruct Alloc(IntPtr handle, int size)
		{
			UnixBufferStruct buf;
			buf.@base = Alloc(size);
			buf.length = (IntPtr)size;
			return buf;
		}

		internal static void Free(UnixBufferStruct buf)
		{
			Free(buf.@base);
		}
#if DEBUG
		public static int PointerCount {
			get {
				return pointers.Count;
			}
		}

		unsafe internal static UnixBufferStruct DebugAlloc(IntPtr handle, int size)
		{
			UnixBufferStruct tmp = UV.Alloc(handle, size);
			Console.WriteLine (tmp.@base);
			return tmp;
		}

		public static void PrintPointers()
		{
			var e = pointers.GetEnumerator();
			Console.Write("[");
			if (e.MoveNext()) {
				Console.Write(e.Current);
				while (e.MoveNext()) {
					Console.Write(", ");
					Console.Write(e.Current);
				}
			}
			Console.Write("]");
		}
#endif
	}
}

