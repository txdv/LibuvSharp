using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Prepare : Handle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_init(IntPtr loop, IntPtr prepare);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_start(IntPtr idle, IntPtr callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_stop(IntPtr prepare);

		public Prepare()
			: this(Loop.Default)
		{
		}

		public Prepare(Loop loop)
			: base(loop, UvHandleType.UV_PREPARE)
		{
			int err = uv_prepare_init(loop.Handle, NativeHandle);
			Ensure.Success(err, Loop);
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
			int err = uv_prepare_start(NativeHandle, callback);
			Ensure.Success(err, Loop);
		}

		public void Stop()
		{
			int err = uv_prepare_stop(NativeHandle);
			Ensure.Success(err, Loop);
		}
	}
}

