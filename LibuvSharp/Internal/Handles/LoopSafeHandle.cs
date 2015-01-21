using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class LoopSafeHandle : BaseSafeHandle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_loop_delete(IntPtr ptr);

		protected override bool ReleaseHandleImpl()
		{
			if (handle != Loop.Default.NativeHandle.Handle) {
				uv_loop_delete(handle);
			}
			return true;
		}
	}
}

