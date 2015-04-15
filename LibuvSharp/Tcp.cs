using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class TcpListener : Listener<Tcp>, IBindable<TcpListener, IPEndPoint>, ILocalAddress<IPEndPoint>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public TcpListener()
			: this(Loop.Constructor)
		{
		}

		public TcpListener(Loop loop)
			: base(loop, HandleType.UV_TCP, uv_tcp_init)
		{
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in sockaddr, uint flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in6 sockaddr, uint flags);

		void Bind(IPAddress ipAddress, int port, bool dualstack)
		{
			CheckDisposed();

			UV.Bind(this, uv_tcp_bind, uv_tcp_bind, ipAddress, port, dualstack);
		}

		public void Bind(int port)
		{
			Bind(IPAddress.IPv6Any, port, true);
		}

		public void Bind(IPEndPoint endPoint)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Bind(endPoint.Address, endPoint.Port, false);
		}

		protected override UVStream Create()
		{
			return new Tcp(Loop);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_simultaneos_accepts(IntPtr handle, int enable);

		public bool SimultaneosAccepts {
			set {
				Invoke(uv_tcp_simultaneos_accepts, value ? 1 : 0);
			}
		}

		public IPEndPoint LocalAddress {
			get {
				CheckDisposed();

				return UV.GetSockname(this, NativeMethods.uv_tcp_getsockname);
			}
		}
	}

	public class Tcp : UVStream, IConnectable<Tcp, IPEndPoint>, ILocalAddress<IPEndPoint>, IRemoteAddress<IPEndPoint>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public Tcp()
			: this(Loop.Constructor)
		{
		}

		public Tcp(Loop loop)
			: base(loop, HandleType.UV_TCP, uv_tcp_init)
		{
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, ref sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, ref sockaddr_in6 addr, callback callback);

		public void Connect(IPEndPoint ipEndPoint, Action<Exception> callback)
		{
			CheckDisposed();

			Ensure.ArgumentNotNull(ipEndPoint, "ipEndPoint");
			Ensure.ArgumentNotNull(callback, "callback");
			Ensure.AddressFamily(ipEndPoint.Address);

			ConnectRequest cpr = new ConnectRequest();
			cpr.Callback = (status, cpr2) => Ensure.Success(status, callback);

			int r;
			if (ipEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				sockaddr_in address = UV.ToStruct(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_connect(cpr.Handle, NativeHandle, ref address, CallbackPermaRequest.CallbackDelegate);
			} else {
				sockaddr_in6 address = UV.ToStruct6(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_connect(cpr.Handle, NativeHandle, ref address, CallbackPermaRequest.CallbackDelegate);
			}
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		public bool NoDelay {
			set {
				Invoke(uv_tcp_nodelay, value ? 1 : 0);
			}
		}

		public void SetKeepAlive(bool enable, int delay)
		{
			Invoke(uv_tcp_keepalive, enable ? 1 : 0, delay);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_getpeername(IntPtr handle, IntPtr addr, ref int length);

		public IPEndPoint LocalAddress {
			get {
				CheckDisposed();

				return UV.GetSockname(this, NativeMethods.uv_tcp_getsockname);
			}
		}

		public IPEndPoint RemoteAddress {
			get {
				CheckDisposed();

				return UV.GetSockname(this, uv_tcp_getpeername);
			}
		}
	}
}

