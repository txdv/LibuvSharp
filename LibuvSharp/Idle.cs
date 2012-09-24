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
			: base(loop, UvHandleType.UV_IDLE)
		{
			int r = uv_idle_init(loop.Handle, NativeHandle);
			Ensure.Success(r, loop);
		}

		private Action<IntPtr, int> cb;
		public void Start(Action<int> callback)
		{
			cb = delegate (IntPtr ptr, int status) {
				callback(status);
			};

			Start(Marshal.GetFunctionPointerForDelegate(cb));
		}

		public void Start(Action <Idle, int> callback)
		{
			cb = delegate (IntPtr ptr, int status) {
				callback(this, status);
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

