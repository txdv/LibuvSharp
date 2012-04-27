using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Udp : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_udp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv")]
		internal static extern int uv_udp_bind(IntPtr handle, sockaddr_in sockaddr, short flags);

		[DllImport("uv")]
		internal static extern int uv_udp_bind6(IntPtr handle, sockaddr_in6 sockaddr, short flags);

		Action<IntPtr, IntPtr, UnixBufferStruct, IntPtr, ushort> recv_start_cb;

		public Udp()
			: this(Loop.Default)
		{
		}

		public Udp(Loop loop)
			: base(loop, UvHandleType.Udp)
		{
			int r = uv_udp_init(loop.Handle, handle);
			UV.EnsureSuccess(r);
			// we can't supply just recv_start_callback in Receive
			// because it will create a temporary delegate which could(and will) be garbage collected at any time
			// happens in my case after 10 or 20 calls
			// so we have to reference it, so it won't garbage collect it until the object itself
			// is gone
			recv_start_cb = recv_start_callback;
		}

		public void Bind(IPAddress ipAddress, int port)
		{
			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_udp_bind(handle, UV.uv_ip4_addr(ipAddress.ToString(), port), 0);
			} else {
				r = uv_udp_bind6(handle, UV.uv_ip6_addr(ipAddress.ToString(), port), 0);
			}
			UV.EnsureSuccess(r);
		}
		public void Bind(string ipAddress, int port)
		{
			Bind(IPAddress.Parse(ipAddress), port);
		}
		public void Bind(IPEndPoint ep)
		{
			Bind(ep.Address, ep.Port);
		}

		[DllImport("uv")]
		internal extern static int uv_udp_send(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in addr, Action<IntPtr, int> cb);

		[DllImport("uv")]
		internal extern static int uv_udp_send6(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, sockaddr_in6 addr, Action<IntPtr, int> cb);

		public void Send(IPAddress ipAddress, int port, byte[] data, int length, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.UdpSend);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0] = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_udp_send(cpr.Handle, handle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			} else {
				r = uv_udp_send6(cpr.Handle, handle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), CallbackPermaRequest.StaticEnd);
			}
			UV.EnsureSuccess(r);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Send(ipAddress, port, data, data.Length, callback);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data, int length)
		{
			Send(ipAddress, port, data, length, null);
		}
		public void Send(IPAddress ipAddress, int port, byte[] data)
		{
			Send(ipAddress, port, data, data.Length);
		}

		public void Send(string ipAddress, int port, byte[] data, int length, Action<bool> callback)
		{
			Send(IPAddress.Parse(ipAddress), port, data, length, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, Action<bool> callback)
		{
			Send(ipAddress, port, data, data.Length, callback);
		}
		public void Send(string ipAddress, int port, byte[] data, int length)
		{
			Send(IPAddress.Parse(ipAddress), port, data, length);
		}
		public void Send(string ipAddress, int port, byte[] data)
		{
			Send(IPAddress.Parse(ipAddress), port, data);
		}

		public void Send(IPEndPoint ep, byte[] data, int length, Action<bool> callback)
		{
			Send(ep.Address, ep.Port, data, length, callback);
		}
		public void Send(IPEndPoint ep, byte[] data, Action<bool> callback)
		{
			Send(ep.Address, ep.Port, data, data.Length, callback);
		}
		public void Send(IPEndPoint ep, byte[] data, int length)
		{
			Send(ep.Address, ep.Port, data, length);
		}
		public void Send(IPEndPoint ep, byte[] data)
		{
			Send(ep.Address, ep.Port, data);
		}

		[DllImport("uv")]
		internal extern static int uv_udp_recv_start(IntPtr handle, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct, IntPtr, ushort> callback);

		internal void recv_start_callback(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			int n = (int)nread;

			if (n == 0) {
				return;
			}

			if (OnMessage != null) {
				OnMessage(Loop.buffer.Get(n), UV.GetIPEndPoint(sockaddr));
			}
		}

		bool receive_init = false;
		public void Receive(Action<byte[], IPEndPoint> callback)
		{
			if (!receive_init) {
				int r = uv_udp_recv_start(handle, Loop.buffer.AllocCallback, recv_start_cb);
				UV.EnsureSuccess(r);
				receive_init = true;
			}
			OnMessage += callback;
		}

		public void Receive(Encoding enc, Action<string, IPEndPoint> callback)
		{
			Receive((data, ep) => {
				callback(enc.GetString(data), ep);
			});
		}

		internal event Action<byte[], IPEndPoint> OnMessage = null;
	}
}

