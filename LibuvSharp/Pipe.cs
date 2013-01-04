using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class PipeListener : Listener<Pipe>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public PipeListener()
			: this(Loop.Default)
		{
		}

		unsafe public PipeListener(Loop loop)
			: base(loop, HandleType.UV_NAMED_PIPE)
		{
			// the ipc setting in the listener is irrelevant
			uv_pipe_init(loop.NativeHandle, NativeHandle, 0);
		}

		protected override UVStream Create()
		{
			return new Pipe(Loop);
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

	public class Pipe : UVStream, IOpenFileDescriptor
	{
		unsafe uv_pipe_t *pipe_t;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public Pipe()
			: this(Loop.Default)
		{
		}

		public Pipe(Loop loop)
			: this(loop, false)
		{
		}

		unsafe internal Pipe(Loop loop, bool interProcessCommunication)
			: base(loop, HandleType.UV_NAMED_PIPE)
		{
			uv_pipe_init(loop.NativeHandle, NativeHandle, interProcessCommunication ? 1 : 0);
			pipe_t = (uv_pipe_t *)(this.NativeHandle.ToInt64() + Handle.Size(HandleType.UV_NAMED_PIPE) - sizeof(uv_pipe_t));
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_open(IntPtr handle, int fd);

		public void Open(IntPtr fd)
		{
			int r = uv_pipe_open(NativeHandle, fd.ToInt32());
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
			Ensure.ArgumentNotNull(name, "name");
			Ensure.ArgumentNotNull(callback, "callback");

			ConnectRequest cpr = new ConnectRequest();
			Pipe pipe = this;

			cpr.Callback = (status, cpr2) => {
				if (status == 0) {
					callback(null);
				} else {
					callback(Ensure.Success(Loop, name));
				}
			};

			uv_pipe_connect(cpr.Handle, pipe.NativeHandle, name, ConnectRequest.StaticEnd);
		}
	}

	public class IPCPipe : Pipe
	{
		public IPCPipe()
			: this(Loop.Default)
		{
		}

		public IPCPipe(Loop loop)
			: base(loop, true)
		{
		}

		[DllImport("uv", EntryPoint = "uv_write2", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_write2_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, IntPtr sendHandle, callback callback);

		[DllImport("uv", EntryPoint = "uv_write2", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_write2_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, IntPtr sendHandle, callback callback);

		public void Write(UVStream stream, byte[] data, int index, int count, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_WRITE);
			cpr.Callback = (status, cpr2) => {
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
				r = uv_write2_unix(cpr.Handle, NativeHandle, buf, 1, stream.NativeHandle, CallbackPermaRequest.StaticEnd);
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, count);
				r = uv_write2_win(cpr.Handle, NativeHandle, buf, 1, stream.NativeHandle, CallbackPermaRequest.StaticEnd);
			}

			Ensure.Success(r, Loop);
		}
		public void Write(UVStream stream, byte[] data, int index, Action<bool> callback)
		{
			Write(stream, data, index, data.Length - index, callback);
		}
		public void Write(UVStream stream, byte[] data, Action<bool> callback)
		{
			Write(stream, data, 0, data.Length, callback);
		}
		public void Write(UVStream stream, byte[] data, int index, int count)
		{
			Write(stream, data, index, count, null);
		}
		public void Write(UVStream stream, byte[] data, int index)
		{
			Write(stream, data, index, data.Length - index, null);
		}
		public void Write(UVStream stream, byte[] data)
		{
			Write(stream, data, 0, data.Length, null);
		}
	}
}
