using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_buf_t
	{
		public IntPtr field0;
		public IntPtr field1;

		internal uv_buf_t(IntPtr pointer, int length)
		{
			if (UV.isUnix) {
				field0 = pointer;
				field1 = (IntPtr)length;
			} else {
				field0 = (IntPtr)length;
				field1 = pointer;
			}
		}
	}
}

