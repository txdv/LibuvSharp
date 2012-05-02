using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Idle : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_idle_init(IntPtr loop, IntPtr idle);

		[DllImport("uv")]
		internal static extern int uv_idle_start(IntPtr idle, IntPtr callback);

		[DllImport("uv")]
		internal static extern int uv_idle_stop(IntPtr loop);

		public Idle()
			: this(Loop.Default)
		{
		}

		public Idle(Loop loop)
			: base(loop, UvHandleType.Idle)
		{
			int r = uv_idle_init(loop.Handle, handle);
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
			int r = uv_idle_start(handle, callback);
			Ensure.Success(r, Loop);
		}

		public void Stop()
		{
			int r = uv_idle_stop(handle);
			Ensure.Success(r, Loop);
		}

	}
}

