using System;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Tcp : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_tcp_init(IntPtr loop, IntPtr handle);

		[DllImport("uv")]
		internal static extern int uv_tcp_nodelay(IntPtr handle, int enable);

		[DllImport("uv")]
		internal static extern int uv_tcp_keepalive(IntPtr handle, int enable, int delay);

		[DllImport("uv")]
		internal static extern int uv_tcp_simultaneos_accepts(IntPtr handle, int enable);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind(IntPtr handle, sockaddr_in sockaddr);

		[DllImport("uv")]
		internal static extern int uv_tcp_bind6(IntPtr handle, sockaddr_in6 sockaddr);

		[DllImport("uv")]
		unsafe internal static extern int uv_tcp_getsockname(IntPtr handle, sockaddr_in6 *name, IntPtr length);

		[DllImport("uv")]
		unsafe internal static extern int uv_tcp_getpeername(IntPtr handle, sockaddr_in6 *name, IntPtr length);

		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect(IntPtr req, IntPtr handle, sockaddr_in addr, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_tcp_connect6(IntPtr req, IntPtr handle, sockaddr_in6 addr, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		public Loop Loop { get; private set; }

		internal Tcp(IntPtr handle)
			: base(handle)
		{
		}

		public Tcp()
			: this(Loop.Default)
		{
		}

		public Tcp(Loop loop)
			: base(UvHandleType.Tcp)
		{
			Loop = loop;
			uv_tcp_init(loop.ptr, handle);
			listen_cb = listen_callback;
			DefaultBacklog = 128;
		}

		public int DefaultBacklog { get; set; }

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

		unsafe public IPEndPoint Sockname {
			get {
				sockaddr_in6 addr;
				sockaddr_in6 *addrptr = &addr;
				IntPtr length;
				uv_tcp_getsockname(handle, addrptr, length);
				return UV.GetIPEndPoint((IntPtr)addrptr);
			}
		}

		unsafe public IPEndPoint Peername {
			get {
				sockaddr_in6 addr;
				sockaddr_in6 *addrptr = &addr;
				IntPtr length;
				uv_tcp_getpeername(handle, addrptr, length);
				return UV.GetIPEndPoint((IntPtr)addrptr);
			}
		}

		unsafe public void Listen(int backlog, Func<bool> accept, Action<TCPStream> callback)
		{
			OnListen += callback;
			if (accept()) {
				int r = uv_listen(handle, backlog, listen_cb);
				UV.EnsureSuccess(r);
			}
		}

		public void Listen(Func<bool> accept, Action<TCPStream> callback)
		{
			Listen(128, accept, callback);
		}

		public void Listen(int backlog, Action<TCPStream> callback)
		{
			Listen(backlog, alwaysAccept, callback);
		}

		public void Listen(Action<TCPStream> callback)
		{
			Listen(alwaysAccept, callback);
		}

		static Tcp()
		{
			alwaysAccept = AlwaysAccept;
		}

		private static Func<bool> alwaysAccept;

		private static bool AlwaysAccept()
		{
			return true;
		}

		Action<IntPtr, int> listen_cb;
		internal void listen_callback(IntPtr req, int status)
		{
			Tcp stream = new Tcp(Loop);
			uv_accept(req, stream.handle);
			OnListen(new TCPStream(stream.handle));
		}

		Action<TCPStream> OnListen = null;

		public void Connect(string ipAddress, int port, Action<TCPStream> callback)
		{
			Connect(IPAddress.Parse(ipAddress), port, callback);
		}

		public void Connect(IPEndPoint ep, Action<TCPStream> callback)
		{
			Connect(ep.Address, ep.Port, callback);
		}

		public void Connect(IPAddress ipAddress, int port, Action<TCPStream> callback)
		{
			ConnectRequest cpr = new ConnectRequest();
			cpr.Callback += (status, cpr2) => {
				callback(status == 0 ? new TCPStream(cpr.ConnectHandle) : null);
			};

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(cpr.Handle, handle, UV.uv_ip4_addr(ipAddress.ToString(), port), cpr.End);
			} else {
				r = uv_tcp_connect6(cpr.Handle, handle, UV.uv_ip6_addr(ipAddress.ToString(), port), cpr.End);
			}
			UV.EnsureSuccess(r);
		}
	}

	public class TCPStream : Tcp, IStream
	{
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct> read_callback);

		[DllImport ("uv")]
		internal static extern int uv_read_stop(IntPtr stream);

		[DllImport("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, Action<IntPtr, int> callback);

		ByteBuffer buffer = new ByteBuffer();

		public TCPStream(IntPtr handle)
			: base(handle)
		{
		}

		public void Resume()
		{
			int r = uv_read_start(handle, buffer.Alloc, read_callback);
			UV.EnsureSuccess(r);
		}

		public void Pause()
		{
			int r = uv_read_stop(handle);
			UV.EnsureSuccess(r);
		}

		internal void read_callback(IntPtr stream, IntPtr size, UnixBufferStruct buf)
		{
			if (size.ToInt64() == 0) {
				return;
			} else if (size.ToInt64() < 0) {
				Close();
				return;
			}

			int length = (int)size;

			if (OnRead != null) {
				OnRead(buffer.Get(length));
			}
		}

		public void Read(Encoding enc, Action<string> callback)
		{
			OnRead += (data) => callback(enc.GetString(data));
		}

		public void Read(Action<byte[]> callback)
		{
			OnRead += callback;
		}

		private Action<byte[]> OnRead;

		unsafe public void Write(byte[] data, int length, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.Write);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0] = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

			int r = uv_write(cpr.Handle, handle, buf, 1, cpr.End);
			UV.EnsureSuccess(r);
		}
		public void Write(byte[] data, int length)
		{
			Write(data, length, null);
		}

		public void Write(byte[] data, Action<bool> callback)
		{
			Write(data, data.Length, callback);
		}
		public void Write(byte[] data)
		{
			Write(data, null);
		}

		public void Write(Encoding enc, string text, Action<bool> callback)
		{
			Write(enc.GetBytes(text), callback);
		}
		public void Write(Encoding enc, string text)
		{
			Write(enc, text, null);
		}
	}
}

