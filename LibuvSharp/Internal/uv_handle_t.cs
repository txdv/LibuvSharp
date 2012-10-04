using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct uv_handle_t
	{
		// public
		public IntPtr close_cb;
		public IntPtr data;
		// read only
		public IntPtr loop;
		public HandleType type;
		// private
		// TODO: implement nginx queue
	}
}

