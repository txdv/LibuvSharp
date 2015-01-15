using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Async : Handle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void async_cb(IntPtr handle, int status);

		[DllImport("uv", CallingConvention=CallingConvention.Cdecl)]
		internal static extern int uv_async_init(LoopSafeHandle loop, IntPtr handle, IntPtr callback);

		public Async()
			: this(Loop.Constructor)
		{
		}

		async_cb cb;
		public Async(Loop loop)
			: base(loop, HandleType.UV_ASYNC)
		{
			cb = (_, status) => {
				OnCallback();
			};

			uv_async_init(loop.NativeHandle, NativeHandle, Marshal.GetFunctionPointerForDelegate(cb));
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_async_send(IntPtr handle);

		public void Send()
		{
			uv_async_send(NativeHandle);
		}

		public event Action Callback;

		public void OnCallback()
		{
			if (Callback != null) {
				Callback();
			}
		}
	}
}

