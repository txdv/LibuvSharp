using System;

namespace LibuvSharp
{
	public static class IOpenFileDescriptorExtensions
	{
		public static void Open(this IOpenFileDescriptor open, int fd)
		{
			open.Open((IntPtr)fd);
		}
	}
}

