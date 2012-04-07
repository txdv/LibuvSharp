using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class BasePipe : Handle
	{
		[DllImport("uv")]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public BasePipe(Loop loop, bool interProcessCommunication)
			: base(loop, UvHandleType.NamedPipe)
		{
			int r = uv_pipe_init(Loop.Handle, handle, (interProcessCommunication ? 1 : 0));
			UV.EnsureSuccess(r);
		}
	}

	public class Pipe : BasePipe
	{
		public Stream Stream { get; protected set; }

		internal Pipe(Loop loop, bool interProcessCommunication)
			: base(loop, interProcessCommunication)
		{
			Stream = new Stream(loop, handle);
		}

		[DllImport("uv")]
		static extern void uv_pipe_open(IntPtr handle, int file);

		public Pipe(int fd)
			: this(fd, false)
		{
		}

		public Pipe(int fd, bool interProcessCommunication)
			: this(Loop.Default, fd)
		{
		}

		public Pipe(Loop loop, int fd)
			: this(loop, fd, false)
		{
		}

		public Pipe(Loop loop, int fd, bool interProcessCommunication)
			: base(loop, interProcessCommunication)
		{
			uv_pipe_open(handle, fd);
		}

		[DllImport("uv")]
		static extern void uv_pipe_connect(IntPtr req, IntPtr handle, string name, Action<IntPtr, int> connect_cb);

		public static void Connect(string name, Action<Pipe> callback)
		{
			Connect(false, name, callback);
		}
		public static void Connect(bool interProcessCommunication, string name, Action<Pipe> callback)
		{
			Connect(Loop.Default, interProcessCommunication, name, callback);
		}
		public static void Connect(Loop loop, string name, Action<Pipe> callback)
		{
			Connect(loop, false, name, callback);
		}
		public static void Connect(Loop loop, bool interProcessCommunication, string name, Action<Pipe> callback)
		{
			ConnectRequest cpr = new ConnectRequest();
			Pipe pipe = new Pipe(loop, interProcessCommunication);

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(pipe);
				} else {
					pipe.Close();
					pipe.Dispose();
					callback(null);
				}
			};

			uv_pipe_connect(cpr.Handle, pipe.handle, name, ConnectRequest.StaticEnd);
		}
	}

	public class PipeServer : BasePipe
	{
		static Func<bool> AlwaysAcceptCallback { get; set; }

		static bool AlwaysAccept()
		{
			return true;
		}

		static PipeServer()
		{
			AlwaysAcceptCallback = AlwaysAccept;
		}

		public int DefaultBacklog { get; set; }

		public PipeServer()
			: this(Loop.Default)
		{
		}

		public PipeServer(bool interProcessCommunication)
			: this(Loop.Default, interProcessCommunication)
		{
		}

		public PipeServer(Loop loop)
			: this(loop, false)
		{
		}

		public PipeServer(Loop loop, bool interProcessCommunication)
			: base(loop, interProcessCommunication)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
		}

		[DllImport("uv")]
		static extern int uv_pipe_bind(IntPtr handle, string name);

		public void Bind(string name)
		{
			int r = uv_pipe_bind(handle, name);
			UV.EnsureSuccess(r);
		}

		[DllImport("uv")]
		internal static extern int uv_listen(IntPtr stream, int backlog, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		Action<IntPtr, int> listen_cb;
		void listen_callback(IntPtr req, int status)
		{
			Pipe pipe = new Pipe(Loop, false);
			uv_accept(req, pipe.handle);
			OnListen(pipe);
		}

		event Action<Pipe> OnListen;

		unsafe public void Listen(int backlog, Func<bool> accept, Action<Pipe> callback)
		{
			OnListen += callback;
			if (accept()) {
				int r = uv_listen(handle, backlog, listen_cb);
				UV.EnsureSuccess(r);
			}
		}

		public void Listen(Func<bool> accept, Action<Pipe> callback)
		{
			Listen(DefaultBacklog, accept, callback);
		}

		public void Listen(int backlog, Action<Pipe> callback)
		{
			Listen(backlog, AlwaysAcceptCallback, callback);
		}

		public void Listen(Action<Pipe> callback)
		{
			Listen(AlwaysAcceptCallback, callback);
		}

		[DllImport("uv")]
		internal static extern int uv_pipe_pending_instances(IntPtr handle, int count);

		public int PendingInstances {
			set {
				uv_pipe_pending_instances(handle, value);
			}
		}
	}

}
