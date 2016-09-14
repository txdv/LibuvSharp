using System;

namespace LibuvSharp
{
	public interface IFileDescriptor
	{
		void Open(IntPtr socket);
		IntPtr FileDescriptor { get; }
	}
}

