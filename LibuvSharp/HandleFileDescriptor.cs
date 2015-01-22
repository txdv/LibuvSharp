using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class HandleFileDescriptor : Handle, IFileDescriptor
	{
		internal HandleFileDescriptor(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
		}

		internal HandleFileDescriptor(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal HandleFileDescriptor(Loop loop, HandleType type)
			: this(loop, Handle.Size(type))
		{
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_fileno_windows(IntPtr handle, out IntPtr fd);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
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

		[DllImport("uv", EntryPoint = "uv_pipe_open", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_open_unix(IntPtr handle, int fd);

		[DllImport("uv", EntryPoint = "uv_pipe_open", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_pipe_open_windows(IntPtr handle, IntPtr fd);

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_unix(IntPtr handle, int sock);

		[DllImport("uv", EntryPoint = "uv_udp_open", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_udp_open_windows(IntPtr handle, IntPtr sock);

		public void Open(Func<IntPtr, int, int> unix, Func<IntPtr, IntPtr, int> windows, IntPtr handle, IntPtr fileDescriptor)
		{
			int r;
			if (UV.IsUnix) {
				r = unix(handle, fileDescriptor.ToInt32());
			} else {
				r = windows(handle, fileDescriptor);
			}
			Ensure.Success(r);
		}

		public void Open(IntPtr fileDescriptor)
		{
			switch (HandleType) {
			case HandleType.UV_TCP:
				Open(uv_tcp_open_unix, uv_tcp_open_windows, NativeHandle, fileDescriptor);
				break;
			case HandleType.UV_NAMED_PIPE:
				Open(uv_pipe_open_unix, uv_pipe_open_windows, NativeHandle, fileDescriptor);
				break;
			case HandleType.UV_UDP:
				Open(uv_udp_open_unix, uv_udp_open_windows, NativeHandle, fileDescriptor);
				break;
			default:
				throw new NotSupportedException();
			}
		}
	}
}

