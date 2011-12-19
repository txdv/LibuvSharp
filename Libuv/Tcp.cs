using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Tcp : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv")]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv")]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		[DllImport("uv")]
		internal static extern int uv_tcp_simultaneos_accepts(IntPtr handle, int enable);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind(IntPtr handle, sockaddr_in sockaddr);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind6(IntPtr handle, sockaddr_in6 sockaddr);

		[DllImport("uv")]
		unsafe internal static extern int uv_tcp_getsockname(IntPtr handle, sockaddr_in6 *name, IntPtr length);

		[DllImport("uv")]
		unsafe internal static extern int uv_tcp_getpeername(IntPtr handle, sockaddr_in6 *name, IntPtr length);

		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, Action<IntPtr, int> callback);

		public Tcp(IntPtr handle)
			: base(handle)
		{
		}

		public Tcp(Loop loop)
			: base(UvHandleType.Tcp)
		{
			uv_tcp_init(loop.ptr, handle);
		}

		public bool NoDelay {
			set {
				uv_tcp_nodelay(handle, (value ? 1 : 0));
			}
		}

		public void SetKeepAlive(bool enable, int delay)
		{
			uv_tcp_keepalive(handle, (enable ? 1 : 0), delay);
		}

		public bool SimultaneosAccepts {
			set {
				uv_tcp_simultaneos_accepts(handle, (value ? 1 : 0));
			}
		}

		public void Bind(string ipAddress, int port)
		{
			Bind(IPAddress.Parse(ipAddress), port);
		}
		public void Bind(IPAddress ipAddress, int port)
		{
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				uv_tcp_bind(handle, UV.uv_ip4_addr(ipAddress.ToString(), port));
			} else {
				uv_tcp_bind6(handle, UV.uv_ip6_addr(ipAddress.ToString(), port));
			}
		}
		public void Bind(IPEndPoint ep)
		{
			Bind(ep.Address, ep.Port);
		}

		unsafe public IPEndPoint Sockname {
			get {
				sockaddr_in6 addr;
				sockaddr_in6 *addrptr = &addr;
				IntPtr length;
				uv_tcp_getsockname(handle, addrptr, length);
				return UV.GetIPEndPoint((IntPtr)addrptr);
			}
		}

		unsafe public IPEndPoint Peername {
			get {
				sockaddr_in6 addr;
				sockaddr_in6 *addrptr = &addr;
				IntPtr length;
				uv_tcp_getpeername(handle, addrptr, length);
				return UV.GetIPEndPoint((IntPtr)addrptr);
			}
		}

		unsafe public void Listen(int backlog, Action<IntPtr, int> callback)
		{
			uv_listen(handle, backlog, callback);
		}

		internal Action<Stream> connect_cb;

		public void Connect(IPAddress ipAddress, int port, Action<Stream> callback)
		{
			connect_cb = callback;
			IntPtr req = Marshal.AllocHGlobal(UV.Sizeof(UvRequestType.Connect));
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				uv_tcp_connect(req, handle, UV.uv_ip4_addr(ipAddress.ToString(), port), connect_callback);
			} else {
				uv_tcp_connect6(req, handle, UV.uv_ip6_addr(ipAddress.ToString(), port), connect_callback);
			}
		}

		internal void connect_callback(IntPtr req, int status)
		{
			connect_cb(new Stream(handle));
		}
	}

	public class Stream : Handle
	{
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct> read_callback);

		[DllImport ("uv")]
		internal static extern int uv_read_stop(IntPtr stream);

		[DllImport("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, UnixBufferStruct bufs, int bufcnt, Action<IntPtr, int> callback);

		public Stream(IntPtr handle)
			: base(handle)
		{
		}

		public void Start()
		{
			uv_read_start(handle, UV.Alloc, (stream, size, buf) => {
				Console.WriteLine(size);
			});
		}

		public void Stop()
		{
			uv_read_stop(handle);
		}

		public void Write(byte[] data)
		{
			Write(data, data.Length);
		}

		public void Write(byte[] data, int length)
		{
			IntPtr req = Marshal.AllocHGlobal(UV.Sizeof(UvRequestType.Write));
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);

			UnixBufferStruct buf = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

			uv_write(req, handle, buf, 1, write_callback);
		}

		internal void write_callback(IntPtr req, int status)
		{
			Marshal.FreeHGlobal(req);
		}
	}

	public class TcpSocket
	{
		internal TcpSocket(IntPtr handle)
		{
		}

		public void Start()
		{
		}
	}

	public class TcpServer
	{
		Tcp tcp;
		Action<TcpSocket> callback;

		public TcpServer(Loop loop, Action<TcpSocket> callback)
		{
			this.callback = callback;
			tcp = new Tcp(loop);
		}

		public void Listen(IPAddress ipAddress, int port, int backlog)
		{
			tcp.Bind(ipAddress, port);
			tcp.Listen(backlog, (stream, status) => {
				callback(new TcpSocket(stream));
			});
		}
	}
}

