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
			: base(loop, HandleType.UV_TCP)
		{
			int r = uv_tcp_init(Loop.NativeHandle, NativeHandle);
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in sockaddr, uint flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_bind(IntPtr handle, ref sockaddr_in6 sockaddr, uint flags);

		public void Bind(IPEndPoint ipEndPoint)
		{
			Ensure.ArgumentNotNull(ipEndPoint, "ipEndPoint");
			int r;
			if (ipEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				sockaddr_in address = UV.ToStruct(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_bind(NativeHandle, ref address, 0);
			} else if (ipEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
				sockaddr_in6 address = UV.ToStruct6(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_bind(NativeHandle, ref address, 0);
			} else {
				throw new ArgumentException("ipEndPoint must be either an ipv4 or ipv6", "ipEndPoint");
			}
			Ensure.Success(r);
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

	public class Tcp : UVStream, IConnectable<Tcp, IPEndPoint>, ILocalAddress<IPEndPoint>, IRemoteAddress<IPEndPoint>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		public Tcp()
			: this(Loop.Constructor)
		{
		}

		public Tcp(Loop loop)
			: base(loop, HandleType.UV_TCP)
		{
			uv_tcp_init(loop.NativeHandle, NativeHandle);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, ref sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, ref sockaddr_in6 addr, callback callback);

		public void Connect(IPEndPoint ipEndPoint, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipEndPoint, "ipEndPoint");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			cpr.Callback = (status, cpr2) => Ensure.Success(status, callback);

			int r;
			if (ipEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				sockaddr_in address = UV.ToStruct(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_connect(cpr.Handle, NativeHandle, ref address, CallbackPermaRequest.CallbackDelegate);
			} else if (ipEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
				sockaddr_in6 address = UV.ToStruct6(ipEndPoint.Address.ToString(), ipEndPoint.Port);
				r = uv_tcp_connect(cpr.Handle, NativeHandle, ref address, CallbackPermaRequest.CallbackDelegate);
			} else {
				throw new ArgumentException("ipEndPoint must be either an ipv4 or ipv6", "ipEndPoint");
			}
			Ensure.Success(r);
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
				Ensure.Success(r);
				return UV.GetIPEndPoint(ptr);
			}
		}
	}
}

