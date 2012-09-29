using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class PipeListener : Listener
	{
		unsafe uv_pipe_t *pipe_t;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

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

		unsafe public PipeListener(Loop loop, bool interProcessCommunication)
			: base(loop, UvHandleType.UV_NAMED_PIPE)
		{
			uv_pipe_init(loop.NativeHandle, NativeHandle, interProcessCommunication ? 1 : 0);
			pipe_t = (uv_pipe_t *)(this.NativeHandle.ToInt64() + UV.uv_handle_size(UvHandleType.UV_STREAM));
		}

		unsafe public bool InterProcessCommunication {
			get {
				return pipe_t->rpc >= 0;
			}
		}

		protected override UVStream Create()
		{
			return new Pipe(Loop, InterProcessCommunication);
		}

		[DllImport("uv")]
		static extern int uv_pipe_bind(IntPtr handle, string name);

		public void Bind(string name)
		{
			Ensure.ArgumentNotNull(name, null);
			int r = uv_pipe_bind(NativeHandle, name);
			Ensure.Success(r, Loop);
		}
	}

	public class Pipe : UVStream
	{
		unsafe uv_pipe_t *pipe_t;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public Pipe()
			: this(false)
		{
		}

		public Pipe(bool interProcessCommunication)
			: this(Loop.Default, interProcessCommunication)
		{
		}

		public Pipe(Loop loop)
			: this(loop, false)
		{
		}

		unsafe public Pipe(Loop loop, bool interProcessCommunication)
			: base(loop, UvHandleType.UV_NAMED_PIPE)
		{
			uv_pipe_init(loop.NativeHandle, NativeHandle, interProcessCommunication ? 1 : 0);
			pipe_t = (uv_pipe_t *)(this.NativeHandle.ToInt64() + UV.uv_handle_size(UvHandleType.UV_NAMED_PIPE) - sizeof(uv_pipe_t));
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_open(IntPtr handle, int file);

		public void Open(IntPtr fileDescriptor)
		{
			int r = uv_pipe_open(NativeHandle, fileDescriptor.ToInt32());
			Ensure.Success(r);
		}

		unsafe public bool InterProcessCommunication {
			get {
				return pipe_t->rpc >= 1;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_pipe_connect(IntPtr req, IntPtr handle, string name, callback connect_cb);

		public void Connect(string name, Action<Exception> callback)
		{
			Connect(name, false, callback);
		}
		public void Connect(string name, bool interProcessCommunication, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(name, "name");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			Pipe pipe = this;

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(null);
				} else {
					pipe.Close();
					callback(Ensure.Success(Loop, name));
				}
			};

			uv_pipe_connect(cpr.Handle, pipe.NativeHandle, name, ConnectRequest.StaticEnd);
		}
	}

}
