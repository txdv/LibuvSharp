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

		internal UVStream(Loop loop, UvHandleType type)
			: this(loop, UV.Sizeof(type))
		{
		}

		public void Resume()
		{
			int r;
			if (UV.isUnix) {
				r = uv_read_start_unix(NativeHandle, Loop.buffer.AllocCallbackUnix, read_cb_unix);
			} else {
				r = uv_read_start_win(NativeHandle, Loop.buffer.AllocCallbackWin, read_cb_win);
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
					Close(EndOfStream);
				} else {
					OnError(new UVException(Loop));
					Close();
				}
			} else {
				if (OnRead != null) {
					OnRead(Loop.buffer.Get(size.ToInt32()));
				}
			}
		}

		protected void OnEndOfStream()
		{
			if (EndOfStream != null) {
				EndOfStream();
			}
		}

		public event Action EndOfStream;

		protected void OnError(UVException exception)
		{
			if (Error != null) {
				Error(exception);
			}
		}

		public event Action<UVException> Error;

		public event Action<byte[]> OnRead;

		public void Read(Encoding enc, Action<string> callback)
		{
			OnRead += (data) => callback(enc.GetString(data));
		}

		public void Read(Action<byte[]> callback)
		{
			OnRead += callback;
		}

		public void Write(byte[] data, int offset, int count, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.UV_WRITE);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			IntPtr ptr = datagchandle.AddrOfPinnedObject() + offset;

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
		public void Write(byte[] data, int offset, int count)
		{
			Write(data, offset, count, null);
		}

		public void Write(byte[] data, int count, Action<bool> callback)
		{
			Write(data, 0, count, callback);
		}
		public void Write(byte[] data, int count)
		{
			Write(data, count, null);
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

		public void Shutdown(Action callback)
		{
			var cbr = new CallbackPermaRequest(UvRequestType.UV_SHUTDOWN);
			cbr.Callback = (status, _) => {
				Close(callback);
			};
			uv_shutdown(cbr.Handle, NativeHandle, CallbackPermaRequest.StaticEnd);
		}

		public void Shutdown()
		{
			Shutdown(null);
		}
	}
}

