using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public partial class HandleBase : IFileDescriptor
	{
		[DllImport("uv", EntryPoint = "uv_fileno", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fileno_windows(IntPtr handle, out IntPtr fd);

		[DllImport("uv", EntryPoint = "uv_fileno", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fileno_unix(IntPtr handle, out int fd);

		public IntPtr FileDescriptor {
			get {
				if (UV.IsUnix) {
					int value;
					uv_fileno_unix(NativeHandle, out value);
					return (IntPtr)value;
				} else {
					IntPtr value;
					uv_fileno_windows(NativeHandle, out value);
					return value;
				}
			}
		}

		[DllImport("uv", EntryPoint = "uv_tcp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_open_unix(IntPtr handle, int sock);

		[DllImport("uv", EntryPoint = "uv_tcp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_tcp_open_windows(IntPtr handle, IntPtr sock);

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_unix(IntPtr handle, int sock);

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_windows(IntPtr handle, IntPtr sock);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_open(IntPtr handle, int fd);

		public int Open(Func<IntPtr, int, int> unix, Func<IntPtr, IntPtr, int> windows, IntPtr handle, IntPtr fileDescriptor)
		{
			if (UV.IsUnix) {
				return unix(handle, fileDescriptor.ToInt32());
			} else {
				return windows(handle, fileDescriptor);
			}
		}

		public void Open(IntPtr fileDescriptor)
		{
			int r;
			switch (HandleType) {
			case HandleType.UV_TCP:
				r = Open(uv_tcp_open_unix, uv_tcp_open_windows, NativeHandle, fileDescriptor);
				break;
			case HandleType.UV_UDP:
				r = Open(uv_udp_open_unix, uv_udp_open_windows, NativeHandle, fileDescriptor);
				break;
			case HandleType.UV_NAMED_PIPE:
				r = uv_pipe_open(NativeHandle, fileDescriptor.ToInt32());
				break;
			default:
				throw new NotSupportedException();
			}
			Ensure.Success(r);
		}
	}
}

