using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct WindowsBufferStruct
	{
		internal WindowsBufferStruct(IntPtr @base, int length)
			: this(@base, (ulong)length)
		{
		}

		internal WindowsBufferStruct(IntPtr @base, long length)
			: this(@base, (ulong)length)
		{
		}

		internal WindowsBufferStruct(IntPtr @base, IntPtr length)
			: this(@base, length.ToInt64())
		{
		}

		internal WindowsBufferStruct(IntPtr @base, ulong length)
		{
			this.@base = @base;
			this.length = length;
		}

		internal ulong length;
		internal IntPtr @base;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct UnixBufferStruct
	{
		internal UnixBufferStruct(IntPtr @base, int length)
			: this(@base, (IntPtr)length)
		{
		}

		internal UnixBufferStruct(IntPtr @base, long length)
			: this(@base, (IntPtr)length)
		{
		}

		internal UnixBufferStruct(IntPtr @base, IntPtr length)
		{
			this.@base = @base;
			this.length = length;
		}

		internal IntPtr @base;
		internal IntPtr length;
	}
}

