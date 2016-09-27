using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Check : StartableCallbackHandle
	{
		[DllImport(PlatformApis.LIBUV, CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_check_init(IntPtr loop, IntPtr idle);

		[DllImport(PlatformApis.LIBUV, CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_check_start(IntPtr check, uv_handle_cb callback);

		[DllImport(PlatformApis.LIBUV, CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_check_stop(IntPtr check);

		public Check()
			: this(Loop.Constructor)
		{
		}

		public Check(Loop loop)
			: base(loop, HandleType.UV_IDLE, uv_check_init)
		{
		}

		public override void Start()
		{
			Invoke(uv_check_start);
		}

		public override void Stop()
		{
			Invoke(uv_check_stop);
		}
	}
}

