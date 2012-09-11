using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_fs_t
	{
		public IntPtr data;
		public IntPtr active_queue_prev;
		public IntPtr active_queue_next;
		public UvRequestType type;

		public int fs_type;
		public IntPtr loop;
		public IntPtr cb;
		public IntPtr result;
		public IntPtr ptr;
		public IntPtr path;
		public int error;
	}
}

