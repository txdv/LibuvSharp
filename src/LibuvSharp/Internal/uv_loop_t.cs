using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct uv_loop_t
	{
		public IntPtr data;
		public uint active_handles;
	}
}

