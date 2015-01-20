using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Prepare : Handle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_init(LoopSafeHandle loop, IntPtr prepare);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_start(IntPtr idle, IntPtr callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_prepare_stop(IntPtr prepare);

		public Prepare()
			: this(Loop.Constructor)
		{
		}

		public Prepare(Loop loop)
			: base(loop, HandleType.UV_PREPARE)
		{
			int r = uv_prepare_init(loop.NativeHandle, NativeHandle);
			Ensure.Success(r);
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
			int r = uv_prepare_start(NativeHandle, callback);
			Ensure.Success(r);
		}

		public void Stop()
		{
			int r = uv_prepare_stop(NativeHandle);
			Ensure.Success(r);
		}
	}
}

