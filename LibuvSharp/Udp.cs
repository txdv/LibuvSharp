using System;
using System.Collections.Generic;
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

		ByteBufferAllocatorBase allocator;
		public ByteBufferAllocatorBase ByteBufferAllocator {
			get {
				return allocator ?? Loop.ByteBufferAllocator;
			}
			set {
				allocator = value;
			}
		}

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

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_win(IntPtr handle, IntPtr sock);

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_lin(IntPtr handle, int sock);

		public void Open(IntPtr socket)
		{
			int r;
			if (UV.IsUnix) {
				r = uv_udp_open_lin(NativeHandle, socket.ToInt32());
			} else {
				r = uv_udp_open_win(NativeHandle, socket);
			}
			Ensure.Success(r);
		}

		bool dualstack = false;
		void Bind(IPAddress ipAddress, int port, short flags)
		{
			Ensure.AddressFamily(ipAddress);

			dualstack = (flags & (short)uv_udp_flags.UV_UDP_IPV6ONLY) == 0
				&& ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_udp_bind(NativeHandle, UV.uv_ip4_addr(ipAddress.ToString(), port), 0);
			} else {
				r = uv_udp_bind6(NativeHandle, UV.uv_ip6_addr(ipAddress.ToString(), port), 0);
			}
			Ensure.Success(r, Loop);
		}
		public void Bind(int port)
		{
			Bind(IPAddress.IPv6Any, port, 0);
		}
		public void Bind(IPAddress ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");

			short flags;

			switch (ipAddress.AddressFamily) {
			case System.Net.Sockets.AddressFamily.InterNetworkV6:
				flags = (short)uv_udp_flags.UV_UDP_IPV6ONLY;
			break;
			default:
				flags = 0;
				break;
			}
			Bind(ipAddress, port, flags);
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

			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + index);

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

		public void Send(IPAddress ipAddress, int port, ArraySegment<byte> data, Action<bool> callback)
		{
			Send(ipAddress, port, data.Array, data.Offset, data.Count, callback);
		}
		public void Send(IPAddress ipAddress, int port, ArraySegment<byte> data)
		{
			Send(ipAddress, port, data, null);
		}
		public void Send(string ipAddress, int port, ArraySegment<byte> data, Action<bool> callback)
		{
			Send(ipAddress, port, data.Array, data.Offset, data.Count, callback);
		}
		public void Send(string ipAddress, int port, ArraySegment<byte> data)
		{
			Send(ipAddress, port, data, null);
		}
		public void Send(IPEndPoint ep, ArraySegment<byte> data, Action<bool> callback)
		{
			Send(ep.Address, ep.Port, data, callback);
		}
		public void Send(IPEndPoint ep, ArraySegment<byte> data)
		{
			Send(ep, data, null);
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

			if (Message != null) {
				var ep = UV.GetIPEndPoint(sockaddr);

				if (dualstack && ep.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
					var data = ep.Address.GetAddressBytes();
					if (IsMapping(data)) {
						ep = new IPEndPoint(GetMapping(data), ep.Port);
					}
				}

				var msg = new UdpMessage(
					ep,
					ByteBufferAllocator.Retrieve(n),
					(flags & (short)uv_udp_flags.UV_UDP_PARTIAL) > 0
				);

				Message(msg);
			}
		}

		bool IsMapping(byte[] data)
		{
			if (data.Length != 16) {
				return false;
			}

			for (int i = 0; i < 10; i++) {
				if (data[i] != 0) {
					return false;
				}
			}

			return data[10] == data[11] && data[11] == 0xff;
		}

		IPAddress GetMapping(byte[] data)
		{
			var ip = new byte[4];
			for (int i = 0; i < 4; i++) {
				ip[i] = data[12 + i];
			}
			return new IPAddress(data);
		}

		bool receiving = false;
		public void Resume()
		{
			if (receiving) {
				return;
			}

			int r;
			if (UV.isUnix) {
				r = uv_udp_recv_start_unix(NativeHandle, ByteBufferAllocator.AllocCallbackUnix, recv_start_cb_unix);
			} else {
				r = uv_udp_recv_start_win(NativeHandle, ByteBufferAllocator.AllocCallbackWin, recv_start_cb_win);
			}
			Ensure.Success(r, Loop);
			receiving = true;
		}

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_stop(IntPtr handle);

		public void Pause()
		{
			if (!receiving) {
				return;
			}
			int r = uv_udp_recv_stop(NativeHandle);
			Ensure.Success(r);
		}

		public event Action<UdpMessage> Message;

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

