using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct uv_process_t
	{
		public IntPtr exit_cb;
		public int pid;
	}
}

