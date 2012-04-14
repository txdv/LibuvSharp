using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Prepare : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_prepare_init(IntPtr loop, IntPtr prepare);

		[DllImport("uv")]
		internal static extern int uv_prepare_start(IntPtr idle, IntPtr callback);

		[DllImport("uv")]
		internal static extern int uv_prepare_stop(IntPtr prepare);

		public Prepare()
			: this(Loop.Default)
		{
		}

		public Prepare(Loop loop)
			: base(loop, UvHandleType.Prepare)
		{
			int err = uv_prepare_init(loop.Handle, handle);
			UV.EnsureSuccess(err);
		}

		Action<IntPtr, int> cb;

		public void Start(Action<int> callback)
		{
			cb = delegate (IntPtr ptr, int status) {
				callback(status);
			};

			Start(Marshal.GetFunctionPointerForDelegate(cb));
		}

		public void Start(Action<Prepare, int> callback)
		{
			cb = delegate (IntPtr ptr, int status) {
				callback(this, status);
			};

			Start(Marshal.GetFunctionPointerForDelegate(cb));
		}

		internal void Start(IntPtr callback)
		{
			int err = uv_prepare_start(handle, callback);
			UV.EnsureSuccess(err);
		}

		public void Stop()
		{
			int err = uv_prepare_stop(handle);
			UV.EnsureSuccess(err);
		}
	}
}

