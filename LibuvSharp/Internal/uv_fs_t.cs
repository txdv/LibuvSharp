using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_fs_t
	{
		public UvRequestType type;
		public IntPtr data;

		public IntPtr loop;
		public int fs_type;
		public IntPtr cb;
		public IntPtr result;
		public IntPtr ptr;
		public IntPtr path;
		public int error;
	}
}

