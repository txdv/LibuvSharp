using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Stream : Handle, IStream
	{
		[DllImport ("uv")]
		internal static extern int uv_read_start(IntPtr stream, Func<IntPtr, int, UnixBufferStruct> alloc_callback, Action<IntPtr, IntPtr, UnixBufferStruct> read_callback);

		[DllImport ("uv")]
		internal static extern int uv_read_watcher_start(IntPtr stream, Action<IntPtr> read_watcher_callback);

		[DllImport ("uv")]
		internal static extern int uv_read_stop(IntPtr stream);

		[DllImport("uv")]
		internal static extern int uv_write(IntPtr req, IntPtr handle, UnixBufferStruct[] bufs, int bufcnt, Action<IntPtr, int> callback);

		[DllImport("uv")]
		internal static extern int uv_shutdown(IntPtr req, IntPtr handle, Action<IntPtr, int> callback);

		internal Stream(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
			read_cb = read_callback;
		}

		internal Stream(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal Stream(Loop loop, UvHandleType type)
			: this(loop, UV.Sizeof(type))
		{
		}

		public void Resume()
		{
			int r = uv_read_start(handle, Loop.buffer.AllocCallback, read_cb);
			Ensure.Success(r, Loop);
		}

		public void Pause()
		{
			int r = uv_read_stop(handle);
			Ensure.Success(r, Loop);
		}

		Action<IntPtr, IntPtr, UnixBufferStruct> read_cb;
		internal void read_callback(IntPtr stream, IntPtr size, UnixBufferStruct buf)
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

		unsafe public void Write(byte[] data, int length, Action<bool> callback)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.UV_WRITE);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0] = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

			int r = uv_write(cpr.Handle, handle, buf, 1, CallbackPermaRequest.StaticEnd);
			Ensure.Success(r, Loop);
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

		public void Shutdown(Action callback)
		{
			var cbr = new CallbackPermaRequest(UvRequestType.UV_SHUTDOWN);
			cbr.Callback = (status, _) => {
				Close(callback);
			};
			uv_shutdown(cbr.Handle, handle, CallbackPermaRequest.StaticEnd);
		}

		public void Shutdown()
		{
			Shutdown(null);
		}
	}
}

