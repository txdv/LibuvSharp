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

		internal Action<IntPtr, IntPtr, UnixBufferStruct, IntPtr, ushort> recv_start_cb;

		public Udp()
			: this(Loop.Default)
		{
		}

		public Udp(Loop loop)
			: base(UvHandleType.Udp)
		{
			uv_udp_init(loop.ptr, handle);
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

		unsafe internal static void send_callback(IntPtr ptr, int status)
		{
			uv_req_t *req = (uv_req_t *)ptr;
			req_gc_handles *gchandles = (req_gc_handles *)req->data;
			gchandles->data.Free();
			if (gchandles->cb) {
				Action<IntPtr, int> cb = (Action<IntPtr, int>)gchandles->callback.Target;
				cb(ptr, status);
				gchandles->callback.Free();
			}
			UV.Free((IntPtr)req);
		}

		unsafe internal void Send(IPAddress ipAddress, int port, byte[] buffer, Action<IntPtr, int> callback)
		{
			uv_req_t *req = (uv_req_t *)UV.Alloc(UV.RequestSizeof(UvRequestType.UdpSend));

			req_gc_handles *reqgc = (req_gc_handles *)UV.Alloc(sizeof(req_gc_handles));
			req->data = (IntPtr)reqgc;

			reqgc->data = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			if (callback != null) {
				reqgc->callback = GCHandle.Alloc(callback, GCHandleType.Pinned);
				reqgc->cb = true;
			} else {
				reqgc->cb = false;
			}

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0].@base = reqgc->data.AddrOfPinnedObject();
			buf[0].length = (IntPtr)buffer.Length;

			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				uv_udp_send((IntPtr)req, handle, buf, 1, UV.uv_ip4_addr(ipAddress.ToString(), port), send_callback);
			} else {
				uv_udp_send6((IntPtr)req, handle, buf, 1, UV.uv_ip6_addr(ipAddress.ToString(), port), send_callback);
			}
		}
		public void Send(IPAddress ipAddress, int port, byte[] buffer, Action<int> callback)
		{
			Send(ipAddress, port, buffer, (req, status) => {
				callback(status);
			});
		}
		public void Send(IPAddress ipAddress, int port, byte[] buffer, Action callback)
		{
			Send(ipAddress, port, buffer, (p, s) => {
				callback();
			});
		}
		public void Send(IPAddress ipAddress, int port, byte[] buffer)
		{
			Send(ipAddress, port, buffer, (Action<IntPtr, int>)null);
		}

		internal void Send(string ipAddress, int port, byte[] buffer, Action<IntPtr, int> callback)
		{
			Send(IPAddress.Parse(ipAddress), port, buffer, callback);
		}
		public void Send(string ipAddress, int port, byte[] buffer, Action<int> callback)
		{
			Send(IPAddress.Parse(ipAddress), port, buffer, callback);
		}
		public void Send(string ipAddress, int port, byte[] buffer, Action callback)
		{
			Send(IPAddress.Parse(ipAddress), port, buffer, callback);
		}
		public void Send(string ipAddress, int port, byte[] buffer)
		{
			Send(IPAddress.Parse(ipAddress), port, buffer);
		}

		internal void Send(IPEndPoint ep, byte[] buffer, Action<IntPtr, int> callback)
		{
			Send(ep.Address, ep.Port, buffer, callback);
		}
		public void Send(IPEndPoint ep, byte[] buffer, Action<int> callback)
		{
			Send(ep.Address, ep.Port, buffer, callback);
		}	
		public void Send(IPEndPoint ep, byte[] buffer, Action callback)
		{
			Send(ep.Address, ep.Port, buffer, callback);
		}
		public void Send(IPEndPoint ep, byte[] buffer)
		{
			Send(ep.Address, ep.Port, buffer);
		}

		[DllImport("uv")]
		internal extern static int uv_udp_recv_start(IntPtr handle, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct, IntPtr, ushort> callback);

		unsafe internal void recv_start_callback(IntPtr handle, IntPtr nread, UnixBufferStruct buf, IntPtr sockaddr, ushort flags)
		{
			int n = (int)nread;

			if (n == 0) {
				UV.Free(buf.@base);
				return;
			}

			byte[] data = new byte[n];
			Marshal.Copy(buf.@base, data, 0, n);

			if (OnMessage != null) {
				OnMessage(data, UV.GetIPEndPoint(sockaddr));
			}
		}

		bool receive_init = false;
		public void Receive(Action<byte[], IPEndPoint> callback)
		{
			if (!receive_init) {
				uv_udp_recv_start(handle, UV.Alloc, recv_start_cb);
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

