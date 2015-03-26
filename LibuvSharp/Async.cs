using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Async : CallbackHandle
	{
		[DllImport("uv", CallingConvention=CallingConvention.Cdecl)]
		static extern int uv_async_init(IntPtr loop, IntPtr handle, uv_handle_cb callback);

		public Async()
			: this(Loop.Constructor)
		{
		}

		public Async(Loop loop)
			: base(loop, HandleType.UV_ASYNC)
		{
			int r = uv_async_init(loop.NativeHandle, NativeHandle, uv_callback);
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_async_send(IntPtr handle);

		public void Send()
		{
			Invoke(uv_async_send);
		}
	}
}

