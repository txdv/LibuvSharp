using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct uv_loop_t
	{
		public IntPtr data;
		#pragma warning disable 169
		uv_err_t last_err;
		#pragma warning restore 169
		public uint active_handlers;
	}
}

