using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class PipeListener : Listener
	{
		[DllImport("uv")]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public bool InterProcessCommunication { get; protected set; }

		public PipeListener()
			: this(Loop.Default)
		{
		}

		public PipeListener(bool interProcessCommunication)
			: this(Loop.Default, interProcessCommunication)
		{
		}

		public PipeListener(Loop loop)
			: this(loop, false)
		{
		}

		public PipeListener(Loop loop, bool interProcessCommunication)
			: base(loop, UvHandleType.NamedPipe)
		{
			uv_pipe_init(loop.Handle, handle, interProcessCommunication ? 1 : 0);
			InterProcessCommunication = interProcessCommunication;
		}

		protected override Stream Create()
		{
			return new Pipe(Loop, InterProcessCommunication);
		}

		[DllImport("uv")]
		static extern int uv_pipe_bind(IntPtr handle, string name);

		public void Bind(string name)
		{
			Ensure.ArgumentNotNull(name, null);
			int r = uv_pipe_bind(handle, name);
			Ensure.Success(r, Loop);
		}
	}

	public class Pipe : Stream
	{
		[DllImport("uv")]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		internal Pipe(Loop loop, bool interProcessCommunication)
			: base(loop, UvHandleType.NamedPipe)
		{
			uv_pipe_init(loop.Handle, handle, interProcessCommunication ? 1 : 0);
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
			: this(loop, interProcessCommunication)
		{
			uv_pipe_open(handle, fd);
		}

		[DllImport("uv")]
		static extern void uv_pipe_connect(IntPtr req, IntPtr handle, string name, Action<IntPtr, int> connect_cb);

		public static void Connect(string name, Action<Exception, Pipe> callback)
		{
			Connect(name, false, callback);
		}
		public static void Connect(string name, bool interProcessCommunication, Action<Exception, Pipe> callback)
		{
			Connect(Loop.Default, name, interProcessCommunication, callback);
		}
		public static void Connect(Loop loop, string name, Action<Exception, Pipe> callback)
		{
			Connect(loop, name, false, callback);
		}
		public static void Connect(Loop loop, string name, bool interProcessCommunication, Action<Exception, Pipe> callback)
		{
			Ensure.ArgumentNotNull(loop, "loop");
			Ensure.ArgumentNotNull(name, "name");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			Pipe pipe = new Pipe(loop, interProcessCommunication);

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(null, pipe);
				} else {
					pipe.Close();
					callback(Ensure.Success(loop, name), null);
				}
			};

			uv_pipe_connect(cpr.Handle, pipe.handle, name, ConnectRequest.StaticEnd);
		}
	}

}
