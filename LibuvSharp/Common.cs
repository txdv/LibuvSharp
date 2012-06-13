using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LibuvSharp
{
	internal class CallbackPermaRequest : PermaRequest
	{
		public CallbackPermaRequest(int size)
			: this(size, true)
		{
		}

		public CallbackPermaRequest(int size, bool allocate)
			: base(size, allocate)
		{
		}

		public CallbackPermaRequest(UvRequestType type)
			: this(type, true)
		{
		}

		public CallbackPermaRequest(UvRequestType type, bool allocate)
			: this(UV.Sizeof(type), allocate)
		{
		}

		public Action<int, CallbackPermaRequest> Callback { get; set; }

		protected void End(IntPtr ptr, int status)
		{
			Callback(status, this);
			Dispose();
		}

		static public void StaticEnd(IntPtr ptr, int status)
		{
			PermaRequest.GetObject<CallbackPermaRequest>(ptr).End(ptr, status);
		}
	}

	unsafe internal class ConnectRequest : CallbackPermaRequest
	{
		uv_connect_t *connect;

		public ConnectRequest()
			: base(UvRequestType.UV_CONNECT)
		{
			connect = (uv_connect_t *)Handle;
		}

		public IntPtr ConnectHandle {
			get {
				return connect->handle;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct lin_stat
	{
		public ulong dev;
		public uint ino;
		public uint mode;
		public uint nlink;
		public uint uid;
		public uint gid;
		public ulong rdev;
		public int size;
		public int blksize;
		public int blkcnt;
		public int atime;
		public int mtime;
		public int ctime;

		public override string ToString ()
		{
			return string.Format ("dev={0} ino={1} mode={2} nlink={3} uid={4} gid={5} rdev={6} size={7} atime={8} mtime={9} ctime={10}", dev, ino, mode, nlink, uid, gid, rdev, size, atime, mtime, ctime);
		}

	}

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

	public static class UV
	{
		unsafe internal static readonly int PointerSize = sizeof(IntPtr) / 4;

		internal static bool isUnix = (System.Environment.OSVersion.Platform == PlatformID.Unix) || (System.Environment.OSVersion.Platform == PlatformID.MacOSX);
		internal static bool IsUnix { get { return isUnix; } }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static sockaddr_in uv_ip4_addr(string ip, int port);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static sockaddr_in6 uv_ip6_addr(string ip, int port);

		[DllImport("__Internal", EntryPoint = "ntohs", CallingConvention = CallingConvention.Cdecl)]
		internal extern static ushort ntohs_unix(ushort bytes);

		[DllImport("Ws2_32", EntryPoint = "ntohs")]
		internal extern static ushort ntohs_win(ushort bytes);

		internal static ushort ntohs(ushort bytes)
		{
			if (isUnix) {
				return ntohs_unix(bytes);
			} else {
				return ntohs_win(bytes);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip4_name(IntPtr src, byte[] dst, IntPtr size);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip6_name(IntPtr src, byte[] dst, IntPtr size);

		unsafe internal static IPEndPoint GetIPEndPoint(IntPtr sockaddr)
		{
			sockaddr *sa = (sockaddr *)sockaddr;
			byte[] addr = new byte[64];
			if (sa->sin_family == 2) {
				uv_ip4_name(sockaddr, addr, (IntPtr)addr.Length);
			} else {
				uv_ip6_name(sockaddr, addr, (IntPtr)addr.Length);
			}

			int i = 0;
			while (i < addr.Length && addr[i] != 0) {
				i++;
			}

			IPAddress ip = IPAddress.Parse(System.Text.Encoding.ASCII.GetString(addr, 0, i));

			return new IPEndPoint(ip, ntohs(sa->sin_port));
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_handle_size(UvHandleType type);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_req_size(UvRequestType type);

		internal static int Sizeof(UvHandleType type)
		{
			return uv_handle_size(type);
		}

		internal static int Sizeof(UvRequestType type)
		{
			return uv_req_size(type);
		}

#if DEBUG
		static List<IntPtr> pointers = new List<IntPtr>();
#endif

		internal static IntPtr Alloc(UvRequestType type)
		{
			return Alloc(Sizeof(type));
		}

		internal static IntPtr Alloc(UvHandleType type)
		{
			return Alloc(Sizeof(type));
		}

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
#if DEBUG
			if (pointers.Contains(ptr)) {
				pointers.Remove(ptr);
				Marshal.FreeHGlobal(ptr);
			} else {
				Console.WriteLine("{0} not allocated", ptr);
			}
#else
			Marshal.FreeHGlobal(ptr);
#endif
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
			Console.WriteLine("]");
		}
#endif
	}
}

