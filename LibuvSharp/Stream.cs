using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Stream : IStream, IDisposable
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

		internal IntPtr Handle { get; set; }
		GCHandle GCHandle { get; set; }
		public Loop Loop { get; protected set; }

		public Stream(Loop loop, IntPtr handle)
		{
			Loop = loop;
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			Handle = handle;
			read_cb = read_callback;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}

		public void Resume()
		{
			int r = uv_read_start(Handle, Loop.buffer.AllocCallback, read_cb);
			UV.EnsureSuccess(r);
		}

		public void Pause()
		{
			int r = uv_read_stop(Handle);
			UV.EnsureSuccess(r);
		}

		Action<IntPtr, IntPtr, UnixBufferStruct> read_cb;
		internal void read_callback(IntPtr stream, IntPtr size, UnixBufferStruct buf)
		{
			long nread = size.ToInt64();
			if (nread == 0) {
				return;
			} else if (nread < 0) {
				if (nread == -1) {
					OnEndOfStream();
					return;
				} else {
					OnError(new UVException(Loop));
					return;
				}

			}

			int length = (int)size;

			if (OnRead != null) {
				OnRead(Loop.buffer.Get(length));
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
			CallbackPermaRequest cpr = new CallbackPermaRequest(UvRequestType.Write);
			cpr.Callback += (status, cpr2) => {
				datagchandle.Free();
				if (callback != null) {
					callback(status == 0);
				}
			};

			UnixBufferStruct[] buf = new UnixBufferStruct[1];
			buf[0] = new UnixBufferStruct(datagchandle.AddrOfPinnedObject(), length);

			int r = uv_write(cpr.Handle, Handle, buf, 1, CallbackPermaRequest.StaticEnd);
			UV.EnsureSuccess(r);
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
			var cbr = new CallbackPermaRequest(UvRequestType.Shutdown);
			cbr.Callback = (status, _) => {
				if (callback != null) {
					callback();
				}
			};
			uv_shutdown(cbr.Handle, Handle, CallbackPermaRequest.StaticEnd);
		}

		public void Shutdown()
		{
			Shutdown(null);
		}
	}
}

