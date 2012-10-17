using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Udp : Handle, IOpenFileDescriptor
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_win(IntPtr handle, IntPtr nread, WindowsBufferStruct buf, IntPtr sockaddr, ushort flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_unix(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind(IntPtr handle, sockaddr_in sockaddr, short flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind6(IntPtr handle, sockaddr_in6 sockaddr, short flags);

		recv_start_callback_win recv_start_cb_win;
		recv_start_callback_unix recv_start_cb_unix;

		public Udp()
			: this(Loop.Default)
		{
		}

		public Udp(Loop loop)
			: base(loop, HandleType.UV_UDP)
		{
			int r = uv_udp_init(loop.NativeHandle, NativeHandle);
			Ensure.Success(r, loop);
			// we can't supply just recv_start_callback in Receive
			// because it will create a temporary delegate which could(and will) be garbage collected at any time
			// happens in my case after 10 or 20 calls
			// so we have to reference it, so it won't garbage collect it until the object itself
			// is gone
			recv_start_cb_win = recv_start_callback_w;
			recv_start_cb_unix = recv_start_callback_u;

		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open(IntPtr handle, IntPtr sock);

		public void Open(IntPtr socket)
		{
			int r = uv_udp_open(NativeHandle, socket);
			Ensure.Success(r);
		}

		public void Bind(IPAddress ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.AddressFamily(ipAddress);

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_udp_bind(NativeHandle, UV.uv_ip4_addr(ipAddress.ToString(), port), 0);
			} else {
				r = uv_udp_bind6(NativeHandle, UV.uv_ip6_addr(ipAddress.ToString(), port), 0);
			}
			Ensure.Success(r, Loop);
		}
		public void Bind(string ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Bind(IPAddress.Parse(ipAddress), port);
		}
		public void Bind(IPEndPoint endPoint)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Bind(endPoint.Address, endPoint.Port);
		}

		[DllImport("uv", EntryPoint = "uv_udp_send", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, sockaddr_in addr, callback callback);
		[DllImport("uv", EntryPoint = "uv_udp_send", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in addr, callback callback);

		[DllImport("uv", EntryPoint = "uv_udp_send6", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send6_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, sockaddr_in6 addr, callback callback);
		[DllImport("uv", EntryPoint = "uv_udp_send6", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send6_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in6 addr, callback callback);

		public void Send(IPAddress ipAddress, int port, byte[] data, int index, int count, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.AddressFamily(ipAddress);
			Ensure.ArgumentNotNull(data, "data");

			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_UDP_SEND);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			IntPtr ptr = datagchandle.AddrOfPinnedObject() + index;

			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(ptr, count);

				if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					r = uv_udp_send_unix(cpr.Handle, NativeHandle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				} else {
					r = uv_udp_send6_unix(cpr.Handle, NativeHandle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				}
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, count);

				if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					r = uv_udp_send_win(cpr.Handle, NativeHandle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				} else {
					r = uv_udp_send6_win(cpr.Handle, NativeHandle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
				}
			}
			Ensure.Success(r, Loop);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, int index, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, index, data.Length - index, callback);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, 0, data.Length, callback);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, int index, int count)
		{
			Send(ipAddress, port, data, index, count, null);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, int index)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, index, data.Length - index);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, 0, data.Length);
		}

		public void Send(string ipAddress, int port, byte[] data, int index, int count, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Send(IPAddress.Parse(ipAddress), port, data, index, count, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, int index, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, index, data.Length - index, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, 0, data.Length, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, int index, int count)
		{
			Send(ipAddress, port, data, index, count, null);
		}
		public void Send(string ipAddress, int port, byte[] data, int index)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, index, data.Length - index);
		}
		public void Send(string ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(ipAddress, port, data, 0, data.Length);
		}

		public void Send(IPEndPoint endPoint, byte[] data, int index, int count, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			Send(endPoint.Address, endPoint.Port, data, index, count, callback);
		}
		public void Send(IPEndPoint endPoint, byte[] data, int index, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, index, data.Length - index, callback);
		}
		public void Send(IPEndPoint endPoint, byte[] data, Action<bool> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, 0, data.Length, callback);
		}
		public void Send(IPEndPoint endPoint, byte[] data, int index, int count)
		{
			Send(endPoint, data, index, count, null);
		}
		public void Send(IPEndPoint endPoint, byte[] data, int index)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, index, data.Length - index);
		}
		public void Send(IPEndPoint endPoint, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Send(endPoint, data, 0, data.Length);
		}

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_win(IntPtr handle, alloc_callback_win alloc_callback, recv_start_callback_win callback);

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_unix(IntPtr handle, alloc_callback_unix alloc_callback, recv_start_callback_unix callback);

		internal void recv_start_callback_w(IntPtr handle, IntPtr nread, WindowsBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			recv_start_callback(handle, nread, sockaddr, flags);
		}
		internal void recv_start_callback_u(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			recv_start_callback(handle, nread, sockaddr, flags);
		}
		internal void recv_start_callback(IntPtr handle, IntPtr nread, IntPtr sockaddr, ushort flags)
		{
			int n = (int)nread;

			if (n == 0) {
				return;
			}

			OnMessage(UV.GetIPEndPoint(sockaddr), Loop.ByteBufferAllocator.Retrieve(n));
		}

		bool receive_init = false;
		public void Receive(Action<IPEndPoint, ByteBuffer> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");

			if (!receive_init) {
				int r;
				if (UV.isUnix) {
					r = uv_udp_recv_start_unix(NativeHandle, Loop.ByteBufferAllocator.AllocCallbackUnix, recv_start_cb_unix);
				} else {
					r = uv_udp_recv_start_win(NativeHandle, Loop.ByteBufferAllocator.AllocCallbackWin, recv_start_cb_win);
				}
				Ensure.Success(r, Loop);
				receive_init = true;
			}
			Message += callback;
		}

		public void Receive(Encoding encoding, Action<IPEndPoint, string> callback)
		{
			Ensure.ArgumentNotNull(encoding, "encoding");
			Ensure.ArgumentNotNull(callback, "callback");

			Receive((ep, data) => callback(ep, encoding.GetString(data.Buffer, data.Start, data.Length)));
		}

		Action<IPEndPoint, ByteBuffer> Message = null;
		void OnMessage(IPEndPoint endPoint, ByteBuffer data)
		{
			if (Message != null) {
				Message(endPoint, data);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_udp_set_ttl(IntPtr handle, int ttl);

		public byte TTL
		{
			set {
				int r = uv_udp_set_ttl(NativeHandle, (int)value);
				Ensure.Success(r);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_udp_set_broadcast(IntPtr handle, int on);

		public bool Broadcast {
			set {
				int r = uv_udp_set_broadcast(NativeHandle, value ? 1 : 0);
				Ensure.Success(r);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_udp_set_multicast_ttl(IntPtr handle, int ttl);

		public byte MulticastTTL {
			set {
				int r = uv_udp_set_multicast_ttl(NativeHandle, (int)value);
				Ensure.Success(r);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_udp_set_multicast_loop(IntPtr handle, int on);

		public bool MulticastLoop {
			set {
				int r = uv_udp_set_multicast_loop(NativeHandle, value ? 1 : 0);
				Ensure.Success(r);
			}
		}
	}
}

