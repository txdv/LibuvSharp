using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Prepare : StartableCallbackHandle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_prepare_init(IntPtr loop, IntPtr prepare);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_prepare_start(IntPtr prepare, uv_handle_cb callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_prepare_stop(IntPtr prepare);

		public Prepare()
			: this(Loop.Constructor)
		{
		}

		public Prepare(Loop loop)
			: base(loop, HandleType.UV_PREPARE, uv_prepare_init)
		{
		}

		public override void Start()
		{
			Invoke(uv_prepare_start);
		}

		public override void Stop()
		{
			Invoke(uv_prepare_stop);
		}
	}
}

