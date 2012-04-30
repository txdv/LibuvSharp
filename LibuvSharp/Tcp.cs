using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class TcpListener : Listener
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public TcpListener()
			: base(Loop.Default, UvHandleType.Tcp)
		{
			uv_tcp_init(Loop.Handle, handle);
		}

		[DllImport("uv")]
		internal static extern int uv_tcp_bind(IntPtr handle, sockaddr_in sockaddr);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind6(IntPtr handle, sockaddr_in6 sockaddr);

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

		protected override Stream Create()
		{
			return new Tcp(Loop);
		}

		public void Listen(int backlog, Action<Tcp> callback)
		{
			Listen(backlog, (Stream stream) => callback(stream as Tcp));
		}

		public void Listen(Action<Tcp> callback)
		{
			Listen(DefaultBacklog, callback);
		}
	}

	public class Tcp : Stream
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		internal Tcp(Loop loop)
			: base(loop, UvHandleType.Tcp)
		{
			uv_tcp_init(loop.Handle, handle);
		}

		[DllImport("uv")]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, Action<IntPtr, int> callback);


		public static void Connect(Loop loop, IPAddress ipAddress, int port, Action<Tcp> callback)
		{
			ConnectRequest cpr = new ConnectRequest();
			Tcp socket = new Tcp(loop);

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(socket);
				} else {
					socket.Close();
					callback(null);
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
		public static void Connect(Loop loop, string ipAddress, int port, Action<Tcp> callback)
		{
			Connect(loop, IPAddress.Parse(ipAddress), port, callback);
		}
		public static void Connect(Loop loop, IPEndPoint ep, Action<Tcp> callback)
		{
			Connect(loop, ep.Address, ep.Port, callback);
		}
		public static void Connect(IPAddress ipAddress, int port, Action<Tcp> callback)
		{
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(string ipAddress, int port, Action<Tcp> callback)
		{
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(IPEndPoint ep, Action<Tcp> callback)
		{
			Connect(Loop.Default, ep, callback);
		}

		[DllImport("uv")]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv")]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		[DllImport("uv")]
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

		[DllImport("uv")]
		internal static extern int uv_tcp_getsockname(IntPtr handle, IntPtr addr, ref int length);

		[DllImport("uv")]
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

