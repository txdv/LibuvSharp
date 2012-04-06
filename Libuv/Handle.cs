using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	internal class PermaCallback : IDisposable
	{
		public Action Callback { get; protected set; }

		GCHandle GCHandle { get; set; }
		Action cb;

		public PermaCallback(Action callback)
		{
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback()
		{
			cb();
			Dispose();
		}

		~PermaCallback()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}
	}

	public class Handle : IDisposable
	{
		public Loop Loop { get; protected set; }

		GCHandle GCHandle { get; set; }
		internal IntPtr handle;

		internal Handle(Loop loop, IntPtr handle)
		{
			this.handle = handle;
			GCHandle = GCHandle.Alloc(this);
			Loop = loop;
		}

		internal Handle(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal Handle(Loop loop, UvHandleType type)
			: this(loop, UV.Sizeof(type))
		{
		}

		public event Action CloseEvent;

		[DllImport("uv")]
		internal static extern int uv_close(IntPtr handle, Action callback);

		public void Close(Action callback)
		{
			if (handle == IntPtr.Zero) {
				return;
			}

			PermaCallback pc = new PermaCallback(() => {
				if (CloseEvent != null) {
					CloseEvent();
				}

				if (callback != null) {
					callback();
				}

				Dispose();
			});

			int r = uv_close(handle, pc.Callback);
			UV.EnsureSuccess(r);
		}

		public void Close()
		{
			Close(null);
		}

		public bool Closed {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			UV.Free(handle);
			GCHandle.Free();
		}

		[DllImport("uv")]
		internal static extern int uv_is_active(IntPtr handle);

		public bool Active {
			get {
				return uv_is_active(handle) != 0;
			}
		}

		[DllImport("uv")]
		internal static extern int uv_is_readable(IntPtr handle);

		public bool Readable {
			get {
				return uv_is_readable(handle) != 0;
			}
		}

		[DllImport("uv")]
		internal static extern int uv_is_writable(IntPtr handle);

		public bool Writeable {
			get {
				return uv_is_writable(handle) != 0;
			}
		}

		[DllImport("uv")]
		internal static extern int uv_is_closing(IntPtr handle);

		public bool Closing {
			get {
				return uv_is_closing(handle) != 0;
			}
		}
	}
}

