using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Handle : IDisposable
	{
		internal IntPtr handle;

		internal Handle(IntPtr handle)
		{
			this.handle = handle;
		}

		internal Handle(UvHandleType type)
			: this(UV.Sizeof(type))
		{
		}

		internal Handle(int size)
			: this(Marshal.AllocHGlobal(size))
		{
		}

		public event Action CloseEvent;

		[DllImport("uv")]
		internal static extern int uv_close(IntPtr handle, IntPtr callback);

		internal void Close(IntPtr callback)
		{
			if (handle != IntPtr.Zero) {
				int r = uv_close(handle, callback);
				UV.EnsureSuccess(r);
			}
		}

		public void Close(Action callback)
		{
			Action cb = null;
			GCHandle gchandle = GCHandle.Alloc(cb, GCHandleType.Pinned);
			cb = delegate {
				callback();

				if (CloseEvent != null) {
					CloseEvent();
				}

				gchandle.Free();
				UV.Free(handle);
				handle = IntPtr.Zero;
			};

			Close(Marshal.GetFunctionPointerForDelegate(cb));
		}

		public void Close()
		{
			Close(() => { });
		}

		public bool Closed {
			get {
				return handle == IntPtr.Zero;
			}
		}

		#region IDisposable implementation
		public void Dispose()
		{
			Close();
		}
		#endregion
	}
}

