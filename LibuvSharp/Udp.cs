using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Udp : HandleBufferSize, IMessageSender<UdpMessage>, IMessageReceiver<UdpReceiveMessage>
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_win(IntPtr handle, IntPtr nread, ref WindowsBufferStruct buf, IntPtr sockaddr, ushort flags);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void recv_start_callback_unix(IntPtr handle, IntPtr nread, ref UnixBufferStruct buf, IntPtr sockaddr, ushort flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind(IntPtr handle, ref sockaddr_in sockaddr, short flags);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_bind(IntPtr handle, ref sockaddr_in6 sockaddr, short flags);

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
			: this(Loop.Constructor)
		{
		}

		public Udp(Loop loop)
			: base(loop, HandleType.UV_UDP)
		{
			int r = uv_udp_init(loop.NativeHandle, NativeHandle);
			Ensure.Success(r);
			// we can't supply just recv_start_callback in Receive
			// because it will create a temporary delegate which could(and will) be garbage collected at any time
			// happens in my case after 10 or 20 calls
			// so we have to reference it, so it won't garbage collect it until the object itself
			// is gone
			recv_start_cb_win = recv_start_callback_w;
			recv_start_cb_unix = recv_start_callback_u;

		}

		bool dualstack = false;
		void Bind(IPAddress ipAddress, int port, short flags)
		{
			Ensure.AddressFamily(ipAddress);

			dualstack = (flags & (short)uv_udp_flags.UV_UDP_IPV6ONLY) == 0
				&& ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				sockaddr_in address = UV.ToStruct(ipAddress.ToString(), port);
				r = uv_udp_bind(NativeHandle, ref address, 0);
			} else {
				sockaddr_in6 address = UV.ToStruct6(ipAddress.ToString(), port);
				r = uv_udp_bind(NativeHandle, ref address , 0);
			}
			Ensure.Success(r);
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

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int nbufs, ref sockaddr_in addr, callback callback);
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int nbufs, ref sockaddr_in addr, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int nbufs, ref sockaddr_in6 addr, callback callback);
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_send(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int nbufs, ref sockaddr_in6 addr, callback callback);

		public void Send(UdpMessage message, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(message.EndPoint, "message EndPoint");
			Ensure.AddressFamily(message.EndPoint.Address);

			var ipEndPoint = message.EndPoint;
			var data = message.Payload;

			GCHandle datagchandle = GCHandle.Alloc(data.Array, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_UDP_SEND);
			cpr.Callback = (status, cpr2) => {
				datagchandle.Free();
				Ensure.Success(status, callback);
			};

			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + data.Offset);

			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(ptr, data.Count);

				if (ipEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					sockaddr_in address = UV.ToStruct(ipEndPoint.Address.ToString(), ipEndPoint.Port);
					r = uv_udp_send(cpr.Handle, NativeHandle, buf, 1, ref address, CallbackPermaRequest.CallbackDelegate);
				} else {
					sockaddr_in6 address = UV.ToStruct6(ipEndPoint.Address.ToString(), ipEndPoint.Port);
					r = uv_udp_send(cpr.Handle, NativeHandle, buf, 1, ref address, CallbackPermaRequest.CallbackDelegate);
				}
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, data.Count);

				if (ipEndPoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
					sockaddr_in address = UV.ToStruct(ipEndPoint.Address.ToString(), ipEndPoint.Port);
					r = uv_udp_send(cpr.Handle, NativeHandle, buf, 1, ref address, CallbackPermaRequest.CallbackDelegate);
				} else {
					sockaddr_in6 address = UV.ToStruct6(ipEndPoint.Address.ToString(), ipEndPoint.Port);
					r = uv_udp_send(cpr.Handle, NativeHandle, buf, 1, ref address, CallbackPermaRequest.CallbackDelegate);
				}
			}
			Ensure.Success(r);
		}

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_win(IntPtr handle, alloc_callback_win alloc_callback, recv_start_callback_win callback);

		[DllImport("uv", EntryPoint = "uv_udp_recv_start", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_udp_recv_start_unix(IntPtr handle, alloc_callback_unix alloc_callback, recv_start_callback_unix callback);

		internal void recv_start_callback_w(IntPtr handle, IntPtr nread, ref WindowsBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			recv_start_callback(handle, nread, sockaddr, flags);
		}
		internal void recv_start_callback_u(IntPtr handle, IntPtr nread, ref UnixBufferStruct buf, IntPtr sockaddr, ushort flags)
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

				var msg = new UdpReceiveMessage(
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
			Ensure.Success(r);
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

		public event Action<UdpReceiveMessage> Message;

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

