using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Idle : Handle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_init(IntPtr loop, IntPtr idle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_start(IntPtr idle, IntPtr callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_idle_stop(IntPtr loop);

		public Idle()
			: this(Loop.Default)
		{
		}

		public Idle(Loop loop)
			: base(loop, HandleType.UV_IDLE)
		{
			int r = uv_idle_init(loop.NativeHandle, NativeHandle);
			Ensure.Success(r, loop);
		}

		Action<IntPtr, int> cb;
		public void Start(Action callback)
		{
			cb = (ptr, status) => {
				if (callback != null) {
					callback();
				}
			};

			Start(Marshal.GetFunctionPointerForDelegate(cb));
		}

		internal void Start(IntPtr callback)
		{
			int r = uv_idle_start(NativeHandle, callback);
			Ensure.Success(r, Loop);
		}

		public void Stop()
		{
			int r = uv_idle_stop(NativeHandle);
			Ensure.Success(r, Loop);
		}
	}
}

