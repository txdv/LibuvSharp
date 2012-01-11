using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public enum TTYMode : int
	{
		Normal = 0,
		Raw
	}

	public class TTY : Handle
	{
		public Stream Stream { get; private set; }

		public GCHandle StreamGCHandle { get; private set; }

		public Loop Loop { get; private set; }

		public IntPtr FileDescriptor { get; private set; }

		[DllImport("uv")]
		static extern int uv_tty_init(IntPtr loop, IntPtr tty, IntPtr fd, int readable);

		public TTY(Loop loop, IntPtr fd, int readable)
			: base(UvHandleType.TTY)
		{
			Loop = loop;
			FileDescriptor = fd;
			int r = uv_tty_init(loop.Handle, handle, fd, readable);
			UV.EnsureSuccess(r);
			Stream = new Stream(handle);
		}

		[DllImport("uv")]
		static extern int uv_tty_set_mode(IntPtr tty, int mode);

		public TTYMode Mode {
			set {
				int r = uv_tty_set_mode(handle, (int)value);
				UV.EnsureSuccess(r);
			}
		}

		[DllImport("uv")]
		static extern int uv_tty_reset_mode();

		static public void ResetMode()
		{
			int r = uv_tty_reset_mode();
			UV.EnsureSuccess(r);
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

