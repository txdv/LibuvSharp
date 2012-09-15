using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct uv_loop_t
	{
		public IntPtr data;
		uv_err_t last_err;
		public uint active_handlers;
	}
}

