using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class TcpListener : Listener<Tcp>, IBindable<TcpListener, IPEndPoint>, ILocalAddress<IPEndPoint>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(LoopSafeHandle loop, IntPtr handle);

		public TcpListener()
			: this(Loop.Constructor)
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

		public void Bind(IPEndPoint endPoint)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			int r;

			if (endPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_bind(NativeHandle, UV.uv_ip4_addr(endPoint.Address.ToString(), endPoint.Port));
			} else {
				r = uv_tcp_bind6(NativeHandle, UV.uv_ip6_addr(endPoint.Address.ToString(), endPoint.Port));
			}
			Ensure.Success(r, Loop);
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

		public IPEndPoint LocalAddress {
			get {
				return UV.GetSockname(this);
			}
		}
	}

	public class Tcp : UVStream, IConnectable<Tcp, IPEndPoint>, ILocalAddress<IPEndPoint>, IRemoteAddress<IPEndPoint>, IOpenFileDescriptor
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(LoopSafeHandle loop, IntPtr handle);

		public Tcp()
			: this(Loop.Constructor)
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
			Ensure.Success(r, Loop);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, callback callback);

		public void Connect(IPEndPoint endPoint, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			cpr.Callback = (status, cpr2) => Ensure.Success(status, Loop, callback);

			int r;
			if (endPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(cpr.Handle, NativeHandle, UV.uv_ip4_addr(endPoint.Address.ToString(), endPoint.Port), CallbackPermaRequest.StaticEnd);
			} else {
				r = uv_tcp_connect6(cpr.Handle, NativeHandle, UV.uv_ip6_addr(endPoint.Address.ToString(), endPoint.Port), CallbackPermaRequest.StaticEnd);
			}
			Ensure.Success(r, Loop);
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
		internal static extern int uv_tcp_getpeername(IntPtr handle, IntPtr addr, ref int length);

		public IPEndPoint LocalAddress {
			get {
				return UV.GetSockname(this);
			}
		}

		unsafe public IPEndPoint RemoteAddress {
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

