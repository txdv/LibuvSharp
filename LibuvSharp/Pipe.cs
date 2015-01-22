using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class PipeListener : Listener<Pipe>, IBindable<PipeListener, string>, ILocalAddress<string>
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public PipeListener()
			: this(Loop.Constructor)
		{
		}

		unsafe public PipeListener(Loop loop)
			: base(loop, HandleType.UV_NAMED_PIPE)
		{
			// the ipc setting in the listener is irrelevant
			int r = uv_pipe_init(loop.NativeHandle, NativeHandle, 0);
			Ensure.Success(r);
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
			Ensure.Success(r);
			LocalAddress = name;
		}

		public string LocalAddress { get; private set; }
	}

	public class Pipe : UVStream, IConnectable<Pipe, string>, IRemoteAddress<string>, IOpenFileDescriptor
	{
		unsafe uv_pipe_t *pipe_t;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);

		public Pipe()
			: this(Loop.Constructor)
		{
		}

		public Pipe(Loop loop)
			: this(loop, false)
		{
		}

		unsafe internal Pipe(Loop loop, bool interProcessCommunication)
			: base(loop, HandleType.UV_NAMED_PIPE)
		{
			int r = uv_pipe_init(loop.NativeHandle, NativeHandle, interProcessCommunication ? 1 : 0);
			Ensure.Success(r);
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

			cpr.Callback = (status, cpr2) => Ensure.Success(status, callback, name);

			uv_pipe_connect(cpr.Handle, pipe.NativeHandle, name, ConnectRequest.CallbackDelegate);
			RemoteAddress = name;
		}

		public string RemoteAddress { get; private set; }

		protected override void OnComplete()
		{
			RemoteAddress = null;
			base.OnComplete();
		}

		protected override void OnError(Exception exception)
		{
			RemoteAddress = null;
			base.OnError(exception);
		}
	}

	public class IPCPipe : Pipe
	{
		public IPCPipe()
			: this(Loop.Constructor)
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

		public void Write(UVStream stream, byte[] data, int index, int count, Action<Exception> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_WRITE);
			cpr.Callback = (status, cpr2) => {
				datagchandle.Free();
				Ensure.Success(status, callback);
			};

			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + index);

			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(ptr, count);
				r = uv_write2_unix(cpr.Handle, NativeHandle, buf, 1, stream.NativeHandle, CallbackPermaRequest.CallbackDelegate);
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, count);
				r = uv_write2_win(cpr.Handle, NativeHandle, buf, 1, stream.NativeHandle, CallbackPermaRequest.CallbackDelegate);
			}

			Ensure.Success(r);
		}
		public void Write(UVStream stream, byte[] data, int index, Action<Exception> callback)
		{
			Write(stream, data, index, data.Length - index, callback);
		}
		public void Write(UVStream stream, byte[] data, Action<Exception> callback)
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
