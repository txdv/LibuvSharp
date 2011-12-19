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

		public Tcp(IntPtr handle)
			: base(handle)
		{
		}

		public Loop Loop { get; private set; }

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

		unsafe public void Listen(int backlog, Func<bool> accept, Action<Stream> callback)
		{
			OnListen += callback;
			if (accept()) {
				int r = uv_listen(handle, backlog, listen_cb);
				UV.EnsureSuccess(r);
			}
		}

		public void Listen(Func<bool> accept, Action<Stream> callback)
		{
			Listen(128, accept, callback);
		}

		public void Listen(int backlog, Action<Stream> callback)
		{
			Listen(backlog, alwaysAccept, callback);
		}

		public void Listen(Action<Stream> callback)
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
			OnListen(new Stream(stream.handle));
		}

		Action<Stream> OnListen = null;

		internal Action<Stream> connect_cb;

		public void Connect(string ipAddress, int port, Action<Stream> callback)
		{
			Connect(IPAddress.Parse(ipAddress), port, callback);
		}

		public void Connect(IPEndPoint ep, Action<Stream> callback)
		{
			Connect(ep.Address, ep.Port, callback);
		}

		public void Connect(IPAddress ipAddress, int port, Action<Stream> callback)
		{
			connect_cb = callback;
			IntPtr req = UV.Alloc(UV.Sizeof(UvRequestType.Connect));

			int r;
			if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
				r = uv_tcp_connect(req, handle, UV.uv_ip4_addr(ipAddress.ToString(), port), connect_callback);
			} else {
				r = uv_tcp_connect6(req, handle, UV.uv_ip6_addr(ipAddress.ToString(), port), connect_callback);
			}

			try {
				UV.EnsureSuccess(r);
			} catch (Exception e) {
				UV.Free(req);
				throw e;
			}
		}

		unsafe internal void connect_callback(IntPtr req, int status)
		{
			uv_connect_t *connect_req = (uv_connect_t *)req;
			connect_cb(new Stream((IntPtr)connect_req->handle));
		}
	}

	public class Stream : Handle
	{
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct> read_callback);

		[DllImport ("uv")]
		internal static extern int uv_read_stop(IntPtr stream);

		[DllImport("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, Action<IntPtr, int> callback);

		public Stream(IntPtr handle)
			: base(handle)
		{
		}

		public void Start()
		{
			uv_read_start(handle, UV.Alloc, read_callback);
		}

		internal void read_callback(IntPtr stream, IntPtr size, UnixBufferStruct buf)
		{
			if (size.ToInt64() == 0) {
				UV.Free(buf);
				return;
			} else if (size.ToInt64() < 0) {
				return;
			}

			int length = (int)size;
			byte[] data = new byte[length];
			Marshal.Copy(buf.@base, data, 0, length);

			if (OnRead != null) {
				OnRead(data);
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

		public void Stop()
		{
			uv_read_stop(handle);
		}


		unsafe public void Write(byte[] data, int length, Action<int> callback)
		{
			uv_req_t *req = (uv_req_t *)UV.Alloc(UV.Sizeof(UvRequestType.Write));

			req_gc_handles *handles = UV.Create(data, callback);
			req->data = (IntPtr)handles;

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0] = new UnixBufferStruct(handles->data.AddrOfPinnedObject(), length);

			uv_write((IntPtr)req, handle, buf, 1, write_callback);
		}
		public void Write(byte[] data, Action<int> callback)
		{
			Write(data, data.Length, callback);
		}
		public void Write(byte[] data, Action callback)
		{
			Write(data, (status) => { callback(); });
		}
		public void Write(Encoding enc, string text, Action<int> callback)
		{
			Write(enc.GetBytes(text), callback);
		}
		public void Write(Encoding enc, string text, Action callback)
		{
			Write(enc, text, (status) => { callback(); });
		}

		unsafe internal void write_callback(IntPtr req, int status)
		{
			uv_req_t *reqptr = (uv_req_t *)req;
			UV.Finish(reqptr->data, status);
			UV.Free(req);
		}
	}

	public class TcpSocket
	{
		public Stream Stream { get; protected set; }

		internal TcpSocket(IntPtr handle)
		{
			Stream = new Stream(handle);
		}

		public void Start()
		{
		}
	}

	public class TcpServer
	{
		Tcp tcp;
		Action<TcpSocket> callback;

		public TcpServer(Loop loop, Action<TcpSocket> callback)
		{
			this.callback = callback;
			tcp = new Tcp(loop);
		}

		public void Listen(IPAddress ipAddress, int port, int backlog)
		{
			tcp.Bind(ipAddress, port);
			tcp.Listen(backlog, (stream) => {
				//callback(socket);
			});
		}
	}
}

