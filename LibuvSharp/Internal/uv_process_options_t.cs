using System;

namespace LibuvSharp
{
	enum uv_process_flags : uint {
		UV_PROCESS_SETUID = (1 << 0),
		UV_PROCESS_SETGID = (1 << 1),
		UV_PROCESS_WINDOWS_VERBATIM_ARGUMENTS = (1 << 2),
		UV_PROCESS_DETACHED = (1 << 3)
	};

	enum uv_stdio_flags : int {
		UV_IGNORE         = 0x00,
		UV_CREATE_PIPE    = 0x01,
		UV_INHERIT_FD     = 0x02,
		UV_INHERIT_STREAM = 0x04,
		UV_READABLE_PIPE  = 0x10,
		UV_WRITABLE_PIPE  = 0x20
	}

	[StructLayout(LayoutKind.Sequential)]
	struct uv_stdio_container_stream_t {
		public uv_stdio_flags flags;
		public IntPtr stream;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct uv_stdio_container_t {
		public uv_stdio_container_stream_t stdin;
		public uv_stdio_container_stream_t stdout;
		public uv_stdio_container_stream_t stderr;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct uv_process_options_t : IDisposable
	{
		// fields

		public IntPtr exit_cb;
		public IntPtr file;
		public IntPtr args;
		public IntPtr env;
		public IntPtr cwd;

		public uint flags;

		public int uid;
		public int gid;

		public int stdio_count;
		uv_stdio_container_stream_t *stdio;

		// functions

		public uv_process_options_t(ProcessOptions options, Action<int,int> exitCallback)
		{
			if (string.IsNullOrEmpty(options.File)) {
				throw new ArgumentException("file of processoptions can't be null");
			} else {
				file = Marshal.StringToHGlobalAnsi(options.File);
			}

			args = alloc(options.Arguments);
			env = alloc(options.Environment);
			cwd = Marshal.StringToHGlobalAnsi(options.CurrentWorkingDirectory);

			flags = 0;
			stdio_count = 3;
			stdio = (uv_stdio_container_stream_t *)Marshal.AllocHGlobal(sizeof(uv_stdio_container_stream_t));

			int i = 0;
			foreach (var stream in new UVStream[] { options.Stdin, options.Stdout, options.Stderr }) {
				if (stream != null && stream is UVStream) {
					stdio[i].flags = uv_stdio_flags.UV_INHERIT_STREAM;
					stdio[i].stream = stream.NativeHandle;
				}
				i++;
			}

			uid = 0;
			gid = 0;

			var that = this;
			exit_cb = Marshal.GetFunctionPointerForDelegate(new CAction<IntPtr, int, int>((handle, exit_status, term_signal) => {
				exitCallback(exit_status, term_signal);
				that.Dispose();
			}).Callback);
		}

		public void Dispose()
		{
			if (file != IntPtr.Zero) {
				Marshal.FreeHGlobal(file);
				file = IntPtr.Zero;
			}

			free(ref args);
			free(ref env);

			Marshal.FreeHGlobal((IntPtr )stdio);
			stdio = (uv_stdio_container_stream_t *)IntPtr.Zero;

			if (cwd != IntPtr.Zero) {
				Marshal.FreeHGlobal(file);
				cwd = IntPtr.Zero;
			}
		}

		static IntPtr alloc(string[] args)
		{
			if (args == null) {
				return IntPtr.Zero;
			}

			var arr = Marshal.AllocHGlobal((args.Length + 1) * sizeof(IntPtr));
			for (int i = 0; i < args.Length; i++) {
				Marshal.WriteIntPtr(arr, i * sizeof(IntPtr), Marshal.StringToHGlobalAnsi(args[i]));
			}
			Marshal.WriteIntPtr(arr, args.Length * sizeof(IntPtr), IntPtr.Zero);
			return arr;
		}

		static void free(ref IntPtr ptr)
		{
			if (ptr == IntPtr.Zero) {
				return;
			}

			int i = 0;
			IntPtr p = Marshal.ReadIntPtr(ptr);
			while (p != IntPtr.Zero) {
				Marshal.FreeHGlobal(p);
				i++;
				p = Marshal.ReadIntPtr(ptr, i * sizeof(IntPtr));
			}

			Marshal.FreeHGlobal(ptr);
			ptr = IntPtr.Zero;
		}
	}
}

