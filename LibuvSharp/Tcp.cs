using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class TcpListener : Listener<Tcp>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public TcpListener()
			: this(Loop.Default)
		{
		}

		public TcpListener(Loop loop)
			: base(loop, HandleType.UV_TCP)
		{
			uv_tcp_init(Loop.NativeHandle, NativeHandle);
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
			Ensure.AddressFamily(ipAddress);

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_bind(NativeHandle, UV.uv_ip4_addr(ipAddress.ToString(), port));
			} else {
				r = uv_tcp_bind6(NativeHandle, UV.uv_ip6_addr(ipAddress.ToString(), port));
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

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_simultaneos_accepts(IntPtr handle, int enable);

		public bool SimultaneosAccepts {
			set {
				uv_tcp_simultaneos_accepts(NativeHandle, (value ? 1 : 0));
			}
		}
	}

	public class Tcp : UVStream, IOpenFileDescriptor
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public Tcp()
			: this(Loop.Default)
		{
		}

		public Tcp(Loop loop)
			: base(loop, HandleType.UV_TCP)
		{
			uv_tcp_init(loop.NativeHandle, NativeHandle);
		}

		[DllImport("uv", EntryPoint = "uv_tcp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_open_win(IntPtr handle, IntPtr sock);

		[DllImport("uv", EntryPoint = "uv_tcp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_open_lin(IntPtr handle, int sock);

		public void Open(IntPtr socket)
		{
			int r;
			if (UV.IsUnix) {
				r = uv_tcp_open_lin(NativeHandle, socket.ToInt32());
			} else {
				r = uv_tcp_open_win(NativeHandle, socket);
			}
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, callback callback);

		public void Connect(IPAddress ipAddress, int port, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			Tcp socket = this;

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(null);
				} else {
					callback(Ensure.Success(Loop));
				}
			};

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(cpr.Handle, socket.NativeHandle, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			} else {
				r = uv_tcp_connect6(cpr.Handle, socket.NativeHandle, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			}
			Ensure.Success(r, Loop);
		}
		public void Connect(string ipAddress, int port, Action<Exception> callback)
		{
			Connect(IPAddress.Parse(ipAddress), port, callback);
		}
		public void Connect(IPEndPoint endPoint, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Connect(endPoint.Address, endPoint.Port, callback);
		}

		public void Connect(IPEndPoint endPoint, ulong timeout, Action<Exception> callback)
		{
			UVTimer timer = new UVTimer(Loop);

			Connect(endPoint, (ex) => {
				timer.Stop();
				timer.Close();
				if (ex is UVException) {
					if ((ex as UVException).Code == 59) { // ECANCELED
						uv_tcp_init(Loop.NativeHandle, NativeHandle);
						ex = new TimeoutException();
					} else {
						throw ex;
					}
				}
				if (callback != null) {
					callback(ex);
				}
			});

			timer.Start(timeout, () => uv_close(NativeHandle, null));
		}
		public void Connect(IPEndPoint endPoint, TimeSpan timeout, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Connect(endPoint, (ulong)timeout.TotalMilliseconds, callback);
		}
		public void Connect(IPAddress ipAddress, int port, TimeSpan timeout, Action<Exception> callback)
		{
			Connect(new IPEndPoint(ipAddress, port), timeout, callback);
		}
		public void Connect(IPAddress ipAddress, int port, ulong timeout, Action<Exception> callback)
		{
			Connect(ipAddress, port, TimeSpan.FromMilliseconds(timeout), callback);
		}
		public void Connect(string ipAddress, int port, TimeSpan timeout, Action<Exception> callback)
		{
			Connect(IPAddress.Parse(ipAddress), port, timeout, callback);
		}
		public void Connect(string ipAddress, int port, ulong timeout, Action<Exception> callback)
		{
			Connect(ipAddress, port, TimeSpan.FromMilliseconds(timeout), callback);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		public bool NoDelay {
			set {
				uv_tcp_nodelay(NativeHandle, (value ? 1 : 0));
			}
		}

		public void SetKeepAlive(bool enable, int delay)
		{
			uv_tcp_keepalive(NativeHandle, (enable ? 1 : 0), delay);
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
				int r = uv_tcp_getsockname(NativeHandle, ptr, ref length);
				Ensure.Success(r, Loop);
				return UV.GetIPEndPoint(ptr);
			}
		}

		unsafe public IPEndPoint Peername {
			get {
				sockaddr_in6 addr;
				IntPtr ptr = new IntPtr(&addr);
				int length = sizeof(sockaddr_in6);
				int r = uv_tcp_getpeername(NativeHandle, ptr, ref length);
				Ensure.Success(r, Loop);
				return UV.GetIPEndPoint(ptr);
			}
		}
	}
}

