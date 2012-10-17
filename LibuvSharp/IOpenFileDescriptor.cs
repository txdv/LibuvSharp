using System;

namespace LibuvSharp
{
	public interface IOpenFileDescriptor
	{
		void Open(IntPtr socket);
	}
}

