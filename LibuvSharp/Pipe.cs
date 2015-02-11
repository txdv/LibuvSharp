using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class BasePipeListener<TListener, TStream> : Listener<TStream>, ILocalAddress<string>, IBindable<TListener, string>
		where TStream : class, IUVStream
		where TListener : IListener<TStream>
	{
		internal BasePipeListener(Loop loop, bool ipc)
			: base(loop, HandleType.UV_NAMED_PIPE)
		{
			int r = NativeMethods.uv_pipe_init(loop.NativeHandle, NativeHandle, ipc == true ? 1 : 0);
			Ensure.Success(r);
		}

		public void Bind(string name)
		{
			Ensure.ArgumentNotNull(name, null);
			int r = NativeMethods.uv_pipe_bind(NativeHandle, name);
			Ensure.Success(r);
		}

		public string LocalAddress {
			get {
				return UV.ToString(4096, (buffer, length) => NativeMethods.uv_pipe_getsockname(NativeHandle, buffer, ref length)).TrimEnd('\0');
			}
		}
	}

	public class PipeListener : BasePipeListener<PipeListener, Pipe>
	{
		public PipeListener()
			: this(Loop.Constructor)
		{
		}

		public PipeListener(Loop loop)
			: base(loop, false)
		{
		}

		protected override UVStream Create()
		{
			return new Pipe(Loop);
		}
	}

	public class IPCPipeListener : BasePipeListener<IPCPipeListener, IPCPipe>
	{
		public IPCPipeListener()
			: this(Loop.Constructor)
		{
		}

		public IPCPipeListener(Loop loop)
			: base(loop, true)
		{
		}

		protected override UVStream Create()
		{
			return new IPCPipe(Loop);
		}
	}

	public class Pipe : UVStream, IConnectable<Pipe, string>, IRemoteAddress<string>
	{
		unsafe uv_pipe_t *pipe_t;

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
			int r = NativeMethods.uv_pipe_init(loop.NativeHandle, NativeHandle, interProcessCommunication ? 1 : 0);
			Ensure.Success(r);
			pipe_t = (uv_pipe_t *)(this.NativeHandle.ToInt64() + Handle.Size(HandleType.UV_STREAM));
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

		public void Write(Handle handle, ArraySegment<byte> segment, Action<Exception> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_WRITE);
			cpr.Callback = (status, cpr2) => {
				datagchandle.Free();
				Ensure.Success(status, callback);
			};

			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + segment.Offset);

			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(ptr, segment.Count);
				r = uv_write2_unix(cpr.Handle, NativeHandle, buf, 1, handle.NativeHandle, CallbackPermaRequest.CallbackDelegate);
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, segment.Count);
				r = uv_write2_win(cpr.Handle, NativeHandle, buf, 1, handle.NativeHandle, CallbackPermaRequest.CallbackDelegate);
			}

			Ensure.Success(r);
		}

		[DllImport(NativeMethods.libuv, CallingConvention = CallingConvention.Cdecl)]
		public static extern int uv_pipe_pending_count(IntPtr handle);

		[DllImport(NativeMethods.libuv, CallingConvention = CallingConvention.Cdecl)]
		public static extern HandleType uv_pipe_pending_type(IntPtr pipe);

		protected override void OnData(ArraySegment<byte> data)
		{
			var count = uv_pipe_pending_count(NativeHandle);
			if (count-- > 0) {
				var type = uv_pipe_pending_type(NativeHandle);
				Handle handle = null;
				switch (type) {
				case HandleType.UV_UDP:
					handle = new Udp(Loop);
					break;
				case HandleType.UV_TCP:
					handle = new Tcp(Loop);
					break;
				case HandleType.UV_NAMED_PIPE:
					handle = new Pipe(Loop);
					break;
				}
				if (handle != null) {
					int r = NativeMethods.uv_accept(NativeHandle, handle.NativeHandle);
					Ensure.Success(r);
					OnHandleData(handle, data);
				}
			}
			base.OnData(data);
		}

		protected virtual void OnHandleData(Handle handle, ArraySegment<byte> data)
		{
			if (HandleData != null) {
				HandleData(handle, data);
			}
		}

		public event Action<Handle, ArraySegment<byte>> HandleData;
	}
}
