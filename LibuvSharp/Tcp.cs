using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class TcpListener : Listener
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public TcpListener()
			: this(Loop.Default)
		{
		}

		public TcpListener(Loop loop)
			: base(loop, UvHandleType.UV_TCP)
		{
			uv_tcp_init(Loop.Handle, handle);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind(IntPtr handle, sockaddr_in sockaddr);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind6(IntPtr handle, sockaddr_in6 sockaddr);

		public void Bind(string ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Bind(IPAddress.Parse(ipAddress), port);
		}
		public void Bind(IPAddress ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_bind(handle, UV.uv_ip4_addr(ipAddress.ToString(), port));
			} else {
				r = uv_tcp_bind6(handle, UV.uv_ip6_addr(ipAddress.ToString(), port));
			}
			Ensure.Success(r, Loop);
		}
		public void Bind(IPEndPoint endPoint)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Bind(endPoint.Address, endPoint.Port);
		}

		protected override UVStream Create()
		{
			return new Tcp(Loop);
		}

		public void Listen(int backlog, Action<Tcp> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");
			Listen(backlog, (UVStream stream) => callback(stream as Tcp));
		}

		public void Listen(Action<Tcp> callback)
		{
			Listen(DefaultBacklog, callback);
		}
	}

	public class Tcp : UVStream
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		internal Tcp(Loop loop)
			: base(loop, UvHandleType.UV_TCP)
		{
			uv_tcp_init(loop.Handle, handle);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, callback callback);


		public static void Connect(Loop loop, IPAddress ipAddress, int port, Action<Exception, Tcp> callback)
		{
			Ensure.ArgumentNotNull(loop, "loop");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			Tcp socket = new Tcp(loop);

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(null, socket);
				} else {
					socket.Close();
					callback(Ensure.Success(loop), null);
				}
			};

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(cpr.Handle, socket.handle, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			} else {
				r = uv_tcp_connect6(cpr.Handle, socket.handle, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			}
			Ensure.Success(r, loop);
		}
		public static void Connect(Loop loop, string ipAddress, int port, Action<Exception, Tcp> callback)
		{
			Connect(loop, IPAddress.Parse(ipAddress), port, callback);
		}
		public static void Connect(Loop loop, IPEndPoint endPoint, Action<Exception, Tcp> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Connect(loop, endPoint.Address, endPoint.Port, callback);
		}
		public static void Connect(IPAddress ipAddress, int port, Action<Exception, Tcp> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(string ipAddress, int port, Action<Exception, Tcp> callback)
		{
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(IPEndPoint endPoint, Action<Exception, Tcp> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Connect(Loop.Default, endPoint, callback);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_open(IntPtr handle, IntPtr native);
		public static Tcp Open(Loop loop, IntPtr nativeHandle)
		{
			Tcp socket = new Tcp(loop);
			uv_tcp_open(socket.handle, nativeHandle);
			return socket;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_simultaneos_accepts(IntPtr handle, int enable);

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

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_getsockname(IntPtr handle, IntPtr addr, ref int length);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_getpeername(IntPtr handle, IntPtr addr, ref int length);

		unsafe public IPEndPoint Sockname {
			get {
				sockaddr_in6 addr;
				IntPtr ptr = new IntPtr(&addr);
				int length = sizeof(sockaddr_in6);
				int r = uv_tcp_getsockname(handle, ptr, ref length);
				Ensure.Success(r, Loop);
				return UV.GetIPEndPoint(ptr);
			}
		}

		unsafe public IPEndPoint Peername {
			get {
				sockaddr_in6 addr;
				IntPtr ptr = new IntPtr(&addr);
				int length = sizeof(sockaddr_in6);
				int r = uv_tcp_getpeername(handle, ptr, ref length);
				Ensure.Success(r, Loop);
				return UV.GetIPEndPoint(ptr);
			}
		}
	}
}

