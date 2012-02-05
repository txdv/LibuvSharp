using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Libuv
{
	public abstract class Tcp : Handle
	{
		public Loop Loop { get; protected set; }

		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public Tcp(IntPtr handle)
			: base(handle)
		{
		}

		public Tcp(Loop loop)
			: base(UvHandleType.Tcp)
		{
			Loop = loop;
			uv_tcp_init(loop.Handle, handle);
		}
	}

	public class TcpServer : Tcp
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_bind(IntPtr handle, sockaddr_in sockaddr);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind6(IntPtr handle, sockaddr_in6 sockaddr);

		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		static TcpServer()
		{
			AlwaysAcceptCallback = AlwaysAccept;
		}

		static Func<bool> AlwaysAcceptCallback { get; set; }

		static bool AlwaysAccept()
		{
			return true;
		}

		public int DefaultBacklog { get; set; }

		public TcpServer()
			: this(Loop.Default)
		{
		}

		public TcpServer(Loop loop)
			: base(loop)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
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

		Action<IntPtr, int> listen_cb;
		void listen_callback(IntPtr req, int status)
		{
			TcpSocket socket = new TcpSocket(Loop);
			uv_accept(req, socket.handle);
			OnListen(socket);
		}

		event Action<TcpSocket> OnListen;

		unsafe public void Listen(int backlog, Func<bool> accept, Action<TcpSocket> callback)
		{
			OnListen += callback;
			if (accept()) {
				int r = uv_listen(handle, backlog, listen_cb);
				UV.EnsureSuccess(r);
			}
		}

		public void Listen(Func<bool> accept, Action<TcpSocket> callback)
		{
			Listen(DefaultBacklog, accept, callback);
		}

		public void Listen(int backlog, Action<TcpSocket> callback)
		{
			Listen(backlog, AlwaysAcceptCallback, callback);
		}

		public void Listen(Action<TcpSocket> callback)
		{
			Listen(AlwaysAcceptCallback, callback);
		}
	}

	public class TcpSocket : Tcp
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, Action<IntPtr, int> callback);


		public static void Connect(Loop loop, IPAddress ipAddress, int port, Action<TcpSocket> callback)
		{
			ConnectRequest cpr = new ConnectRequest();
			TcpSocket socket = new TcpSocket(loop);

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(socket);
				} else {
					socket.Close();
					socket.Dispose();
					callback(null);
				}
			};

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(cpr.Handle, socket.handle, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			} else {
				r = uv_tcp_connect6(cpr.Handle, socket.handle, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			}
			UV.EnsureSuccess(r);
		}
		public static void Connect(Loop loop, string ipAddress, int port, Action<TcpSocket> callback)
		{
			Connect(loop, IPAddress.Parse(ipAddress), port, callback);
		}
		public static void Connect(Loop loop, IPEndPoint ep, Action<TcpSocket> callback)
		{
			Connect(loop, ep.Address, ep.Port, callback);
		}
		public static void Connect(IPAddress ipAddress, int port, Action<TcpSocket> callback)
		{
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(string ipAddress, int port, Action<TcpSocket> callback)
		{
			Connect(Loop.Default, ipAddress, port, callback);
		}
		public static void Connect(IPEndPoint ep, Action<TcpSocket> callback)
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
				UV.EnsureSuccess(r);
				return UV.GetIPEndPoint(ptr);
			}
		}

		unsafe public IPEndPoint Peername {
			get {
				sockaddr_in6 addr;
				IntPtr ptr = new IntPtr(&addr);
				int length = sizeof(sockaddr_in6);
				int r = uv_tcp_getpeername(handle, ptr, ref length);
				UV.EnsureSuccess(r);
				return UV.GetIPEndPoint(ptr);
			}
		}

		public Stream Stream { get; protected set; }

		public TcpSocket()
			: this(Loop.Default)
		{
		}

		public TcpSocket(Loop loop)
			: base(loop)
		{
			Stream = new Stream(loop, handle);
		}

		public TcpSocket(Loop loop, IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Stream.Dispose();
		}
	}
}

