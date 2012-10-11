using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class UVStream : Handle, IUVStream
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void read_callback_unix(IntPtr a, IntPtr blet, UnixBufferStruct buf);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void read_callback_win(IntPtr a, IntPtr blet, WindowsBufferStruct buf);

		[DllImport("uv", EntryPoint = "uv_read_start", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_start_unix(IntPtr stream, alloc_callback_unix alloc_callback, read_callback_unix read_callback);

		[DllImport("uv", EntryPoint = "uv_read_start", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_start_win(IntPtr stream, alloc_callback_win alloc_callback, read_callback_win read_callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_watcher_start(IntPtr stream, Action<IntPtr> read_watcher_callback);

		[DllImport ("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_read_stop(IntPtr stream);

		[DllImport("uv", EntryPoint = "uv_write", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_write_unix(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, callback callback);

		[DllImport("uv", EntryPoint = "uv_write", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_write_win(IntPtr req, IntPtr handle, WindowsBufferStruct[] bufs, int bufcnt, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_shutdown(IntPtr req, IntPtr handle, callback callback);

		internal UVStream(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
			read_cb_unix = read_callback_u;
			read_cb_win = read_callback_w;
		}

		internal UVStream(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal UVStream(Loop loop, HandleType type)
			: this(loop, Handle.Size(type))
		{
		}

		public void Resume()
		{
			int r;
			if (UV.isUnix) {
				r = uv_read_start_unix(NativeHandle, Loop.ByteBufferAllocator.AllocCallbackUnix, read_cb_unix);
			} else {
				r = uv_read_start_win(NativeHandle, Loop.ByteBufferAllocator.AllocCallbackWin, read_cb_win);
			}
			Ensure.Success(r, Loop);
		}

		public void Pause()
		{
			int r = uv_read_stop(NativeHandle);
			Ensure.Success(r, Loop);
		}

		read_callback_unix read_cb_unix;
		internal void read_callback_u(IntPtr stream, IntPtr size, UnixBufferStruct buf)
		{
			read_callback(stream, size);
		}

		read_callback_win read_cb_win;
		internal void read_callback_w(IntPtr stream, IntPtr size, WindowsBufferStruct buf)
		{
			read_callback(stream, size);
		}

		internal void read_callback(IntPtr stream, IntPtr size)
		{
			long nread = size.ToInt64();
			if (nread == 0) {
				return;
			} else if (nread < 0) {
				if (nread == -1) {
					Close(Complete);
				} else {
					OnError(Ensure.Success(Loop));
					Close();
				}
			} else {
				OnRead(Loop.ByteBufferAllocator.Retrieve(size.ToInt32()));
			}
		}

		protected void OnComplete()
		{
			if (Complete != null) {
				Complete();
			}
		}

		public event Action Complete;

		protected void OnError(Exception exception)
		{
			if (Error != null) {
				Error(exception);
			}
		}

		public event Action<Exception> Error;

		Action<ByteBuffer> ReadEvent;

		void OnRead(ByteBuffer buffer)
		{
			if (ReadEvent != null) {
				ReadEvent(buffer);
			}
		}

		public void Read(Action<ByteBuffer> callback)
		{
			if (callback != null) {
				ReadEvent = callback;
			}
		}

		public void Write(byte[] data, int index, int count, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(RequestType.UV_WRITE);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			IntPtr ptr = datagchandle.AddrOfPinnedObject() + index;

			int r;
			if (UV.isUnix) {
				UnixBufferStruct[] buf = new UnixBufferStruct[1];
				buf[0] = new UnixBufferStruct(ptr, count);
				r = uv_write_unix(cpr.Handle, NativeHandle, buf, 1, CallbackPermaRequest.StaticEnd);
			} else {
				WindowsBufferStruct[] buf = new WindowsBufferStruct[1];
				buf[0] = new WindowsBufferStruct(ptr, count);
				r = uv_write_win(cpr.Handle, NativeHandle, buf, 1, CallbackPermaRequest.StaticEnd);
			}

			Ensure.Success(r, Loop);
		}

		public void Shutdown(Action callback)
		{
			var cbr = new CallbackPermaRequest(RequestType.UV_SHUTDOWN);
			cbr.Callback = (status, _) => {
				Close(callback);
			};
			uv_shutdown(cbr.Handle, NativeHandle, CallbackPermaRequest.StaticEnd);
		}
	}
}

