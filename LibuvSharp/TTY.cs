using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public enum TTYMode : int
	{
		Normal = 0,
		Raw
	}

	public class TTY : Stream
	{
		public IntPtr FileDescriptor { get; private set; }

		[DllImport("uv")]
		static extern int uv_tty_init(IntPtr loop, IntPtr tty, IntPtr fd, int readable);

		public TTY(Loop loop, int fd)
			: this(loop, fd, true)
		{
		}

		public TTY(Loop loop, IntPtr fd)
			: this(loop, fd, true)
		{
		}

		public TTY(Loop loop, int fd, bool readable)
			: this(loop, (IntPtr)fd, readable)
		{
		}

		public TTY(Loop loop, IntPtr fd, bool readable)
			: base(loop, UvHandleType.TTY)
		{
			FileDescriptor = fd;
			int r = uv_tty_init(loop.Handle, handle, fd, (readable ? 1 : 0));
			Ensure.Success(r, Loop);
		}

		[DllImport("uv")]
		static extern int uv_tty_set_mode(IntPtr tty, int mode);

		public TTYMode Mode {
			set {
				int r = uv_tty_set_mode(handle, (int)value);
				Ensure.Success(r, Loop);
			}
		}

		[DllImport("uv")]
		static extern int uv_tty_reset_mode();

		static public void ResetMode()
		{
			int r = uv_tty_reset_mode();
			Ensure.Success(r);
		}

		[DllImport("uv")]
		static extern int uv_tty_get_winsize(IntPtr tty, out int width, out int height);

		public bool GetWindowSize(out int width, out int height)
		{
			int r = uv_tty_get_winsize(handle, out width, out height);
			return r == 0;
		}

		[DllImport("uv")]
		static extern UvHandleType uv_guess_handle(IntPtr fd);
	}
}

