using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class LoopBackend
	{
		LoopSafeHandle nativeHandle;

		internal LoopBackend(Loop loop)
		{
			nativeHandle = loop.NativeHandle;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_backend_fd(LoopSafeHandle loop);

		public int FileDescriptor {
			get {
				return uv_backend_fd(nativeHandle);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_backend_timeout(LoopSafeHandle loop);

		public int Timeout {
			get {
				return uv_backend_timeout(nativeHandle);
			}
		}
	}
}

