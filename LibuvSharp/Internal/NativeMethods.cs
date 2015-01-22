using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal static class NativeMethods
	{
		private const string libuv = "uv";

		[DllImport(libuv, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_listen(IntPtr stream, int backlog, Handle.callback callback);

		[DllImport(libuv, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		[DllImport(libuv, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_getsockname(IntPtr handle, IntPtr addr, ref int length);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void uv_fs_cb(IntPtr IntPtr);

		[DllImport(libuv, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_pipe_init(IntPtr loop, IntPtr handle, int ipc);
	}
}

