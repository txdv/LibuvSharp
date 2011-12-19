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

		[DllImport("uv")]
		internal static extern int uv_close(IntPtr handle, IntPtr callback);

		public void Close(IntPtr callback)
		{
			if (handle != IntPtr.Zero) {
				uv_close(handle, callback);
			}
			handle = IntPtr.Zero;
		}

		public void Close(Action callback)
		{
			Close(Marshal.GetFunctionPointerForDelegate(callback));
		}

		public void Close()
		{
			Close(IntPtr.Zero);
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

