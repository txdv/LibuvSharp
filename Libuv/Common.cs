using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Libuv
{
	unsafe internal class Request<T> : IDisposable where T : struct
	{
		public IntPtr Handle { get; protected set; }

		protected uv_req_t *request;

		public IntPtr Data {
			get {
				return request->data;
			}
			set {
				request->data = value;
			}
		}

		public T Value {
			get {
				T *p = (T *)Data;
				return *p;
			}
			set {
				T *p = (T *)Data;
				*p = value;
			}
		}

		public Request(int size)
			: this(size, true)
		{
		}

		public Request(int size, bool allocate)
			: this(UV.Alloc(size), allocate)
		{
		}

		public Request(IntPtr ptr)
			: this(ptr, true)
		{
		}

		protected Request(IntPtr handle, bool allocate)
		{
			Handle = handle;
			request = (uv_req_t *)handle;

			Data = IntPtr.Zero;

			if (allocate) {
				Data = UV.Alloc(sizeof(T));
			}
		}

		public virtual void Dispose()
		{
			Dispose(true);
		}

		public virtual void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			if (Data != IntPtr.Zero) {
				UV.Free(Data);
			}

			UV.Free(Handle);
		}
	}

	internal class PermaRequest : Request<GCHandle>
	{
		public PermaRequest(int size)
			: this(size, true)
		{
		}

		~PermaRequest()
		{
			Dispose(false);
		}

		public PermaRequest(int size, bool allocate)
			: base(size, allocate)
		{
			Value = GCHandle.Alloc(this, GCHandleType.Normal);
		}

		public override void Dispose(bool disposing)
		{
			Value.Free();
			base.Dispose(disposing);
		}

		unsafe public static T GetObject<T>(IntPtr ptr) where T : class
		{
			uv_req_t *req = (uv_req_t *)ptr.ToPointer();
			GCHandle *gchandle = (GCHandle *)req->data;
			return (gchandle->Target as T);
		}
	}

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
			: base(UvRequestType.Connect)
		{
			connect = (uv_connect_t *)Handle;
		}

		public IntPtr ConnectHandle {
			get {
				return connect->handle;
			}
		}
	}

	unsafe internal class FileSystemRequest : PermaRequest
	{
		private static readonly int Size = UV.Sizeof(UvHandleType.File);

		protected uv_fs_t *fsrequest;

		public FileSystemRequest()
			: this(true)
		{
		}

		public FileSystemRequest(bool allocate)
			: base(Size, allocate)
		{
			fsrequest = (uv_fs_t *)Handle;
		}

		public Action<Exception, FileSystemRequest> Callback { get; set; }

		[DllImport("uv")]
		private static extern void uv_fs_req_cleanup(IntPtr req);

		public override void Dispose(bool disposing)
		{
			uv_fs_req_cleanup(Handle);
			base.Dispose(disposing);
		}

		public IntPtr Result {
			get {
				return fsrequest->result;
			}
		}

		public int Error {
			get {
				return fsrequest->error;
			}
		}

		public IntPtr Pointer {
			get {
				return fsrequest->ptr;
			}
		}

		public void End(IntPtr ptr)
		{
			// good idea when you have only the pointer, but no need for it ...
			// var fsr = new FileSystemRequest(ptr, false).Value.Target as FileSystemRequest;
			Exception e = null;
			if (Result == (IntPtr)(-1)) {
				uv_err_t error = new uv_err_t(Error);
				e = new Exception(string.Format("{0}: {1}", error.Name, error.Description));
			}

			if (Callback != null) {
				Callback(e, this);
			}
			Dispose();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe public struct uv_err_t
	{
		public uv_err_t(int errorcode)
		{
			this.code = (uv_err_code)errorcode;
			this.sys_errno_ = 0;
		}

		[DllImport("uv")]
		private static extern sbyte *uv_strerror(uv_err_t error);

		[DllImport("uv")]
		private static extern sbyte *uv_err_name(uv_err_t error);

		public uv_err_code code;
		int sys_errno_;

		public string Description {
			get {
				return new string(uv_strerror(this));
			}
		}

		public string Name {
			get {
				return new string(uv_err_name(this));
			}
		}
	}
	public enum uv_err_code
	{
		UV_UNKNOWN = -1,
		UV_OK = 0,
		UV_EOF,
		UV_EACCESS,
		UV_EAGAIN,
		UV_EADDRINUSE,
		UV_EADDRNOTAVAIL,
		UV_EAFNOSUPPORT,
		UV_EALREADY,
		UV_EBADF,
		UV_EBUSY,
		UV_ECONNABORTED,
		UV_ECONNREFUSED,
		UV_ECONNRESET,
		UV_EDESTADDRREQ,
		UV_EFAULT,
		UV_EHOSTUNREACH,
		UV_EINTR,
		UV_EINVAL,
		UV_EISCONN,
		UV_EMFILE,
		UV_ENETDOWN,
		UV_ENETUNREACH,
		UV_ENFILE,
		UV_ENOBUFS,
		UV_ENOMEM,
		UV_ENONET,
		UV_ENOPROTOOPT,
		UV_ENOTCONN,
		UV_ENOTSOCK,
		UV_ENOTSUP,
		UV_EPROTO,
		UV_EPROTONOSUPPORT,
		UV_EPROTOTYPE,
		UV_ETIMEDOUT,
		UV_ECHARSET,
		UV_EAIFAMNOSUPPORT,
		UV_EAINONAME,
		UV_EAISERVICE,
		UV_EAISOCKTYPE,
		UV_ESHUTDOWN
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_fs_t
	{
		public UvRequestType type;
		public IntPtr data;

		public IntPtr loop;
		public int fs_type;
		public IntPtr cb;
		public IntPtr result;
		public IntPtr ptr;
		public IntPtr path;
		public int error;
	}


	[StructLayout(LayoutKind.Sequential)]
	internal struct lin_stat
	{
		public long dev;
		public uint ino;
		public uint mode;
		public uint nlink;
		public uint uid;
		public uint gid;
		public ulong rdev;
		public int size;
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

	internal enum UvHandleType : int
	{
		Unknown,

		AresTask,
		Async,
		Check,
		FSEvent,
		Idle,
		NamedPipe,
		Prepare,
		Process,
		Tcp,
		Timer,
		TTY,
		Udp,

		File,
		Private,
		Max,
	};

	internal enum UvRequestType : int
	{
		Unknown,

		Connect,
		Write,
		Shutdown,
		UdpSend,
		FileSystem,
		Work,
		GetAddrInfo,

		Private,
		Max,
	}

	public static class UV
	{
		internal static bool isUnix = (System.Environment.OSVersion.Platform == PlatformID.Unix) || (System.Environment.OSVersion.Platform == PlatformID.MacOSX);
		internal static bool IsUnix { get { return isUnix; } }


		[DllImport ("uv")]
		public static extern uv_err_t uv_last_error(IntPtr loop);

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
		internal static extern int uv_handle_size(UvHandleType type);

		[DllImport("uv")]
		internal static extern int uv_req_size(UvRequestType type);

		internal static int Sizeof(UvHandleType type)
		{
			return uv_handle_size(type);
		}

		internal static int Sizeof(UvRequestType type)
		{
			return uv_req_size(type);
		}

		internal static void EnsureSuccess(int errorCode)
		{
			if (errorCode < 0) {
				throw new Exception(errorCode.ToString());
			}
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
			pointers.Remove(ptr);
#endif
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

