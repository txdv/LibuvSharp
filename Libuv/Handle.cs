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

		public void Close(Action callback)
		{
			if (handle != IntPtr.Zero) {
				uv_close(handle, Marshal.GetFunctionPointerForDelegate(callback));
			}
			handle = IntPtr.Zero;
		}

		public void Close()
		{
			if (handle != IntPtr.Zero) {
				uv_close(handle, IntPtr.Zero);
			}
			handle = IntPtr.Zero;
		}

		#region IDisposable implementation
		public void Dispose()
		{
			Close();
		}
		#endregion
	}
}

