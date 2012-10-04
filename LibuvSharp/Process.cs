using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public enum Signum : int {
		SIGHUP    =  1, // Hangup (POSIX).
		SIGINT    =  2, // Interrupt (ANSI).
		SIGQUIT   =  3, // Quit (POSIX).
		SIGILL    =  4, // Illegal instruction (ANSI).
		SIGTRAP   =  5, // Trace trap (POSIX).
		SIGABRT   =  6, // Abort (ANSI).
		SIGIOT    =  6, // IOT trap (4.2 BSD).
		SIGBUS    =  7, // BUS error (4.2 BSD).
		SIGFPE    =  8, // Floating-point exception (ANSI).
		SIGKILL   =  9, // Kill, unblockable (POSIX).
		SIGUSR1   = 10, // User-defined signal 1 (POSIX).
		SIGSEGV   = 11, // Segmentation violation (ANSI).
		SIGUSR2   = 12, // User-defined signal 2 (POSIX).
		SIGPIPE   = 13, // Broken pipe (POSIX).
		SIGALRM   = 14, // Alarm clock (POSIX).
		SIGTERM   = 15, // Termination (ANSI).
		SIGSTKFLT = 16, // Stack fault.
		SIGCLD    = SIGCHLD, // Same as SIGCHLD (System V).
		SIGCHLD   = 17, // Child status has changed (POSIX).
		SIGCONT   = 18, // Continue (POSIX).
		SIGSTOP   = 19, // Stop, unblockable (POSIX).
		SIGTSTP   = 20, // Keyboard stop (POSIX).
		SIGTTIN   = 21, // Background read from tty (POSIX).
		SIGTTOU   = 22, // Background write to tty (POSIX).
		SIGURG    = 23, // Urgent condition on socket (4.2 BSD).
		SIGXCPU   = 24, // CPU limit exceeded (4.2 BSD).
		SIGXFSZ   = 25, // File size limit exceeded (4.2 BSD).
		SIGVTALRM = 26, // Virtual alarm clock (4.2 BSD).
		SIGPROF   = 27, // Profiling alarm clock (4.2 BSD).
		SIGWINCH  = 28, // Window size change (4.3 BSD, Sun).
		SIGPOLL   = SIGIO, // Pollable event occurred (System V).
		SIGIO     = 29, // I/O now possible (4.2 BSD).
		SIGPWR    = 30, // Power failure restart (System V).
		SIGSYS    = 31, // Bad system call.
		SIGUNUSED = 31
	}

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
		//public IntPtr stdio;
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

	public class ProcessOptions
	{
		public string File { get; set; }
		public string[] Arguments { get; set; }
		public string[] Environment { get; set; }
		public string CurrentWorkingDirectory { get; set; }
		public bool WindowsVerbatimArguments { get; set; }

		public UVStream Stdin { get; set; }
		public UVStream Stdout { get; set; }
		public UVStream Stderr { get; set; }
	}

	public class Process : Handle
	{
		public int ExitCode { get; private set; }
		public int TermSignal { get; private set; }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_get_process_title(IntPtr buffer, IntPtr size);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_set_process_title(string title);

		public static string Title {
			get {
				int size = 512;
				var ptr = Marshal.AllocHGlobal(size);
				uv_get_process_title(ptr, (IntPtr)size);
				string ret = Marshal.PtrToStringAnsi(ptr);
				Marshal.FreeHGlobal(ptr);
				return ret;
			}
			set {
				uv_set_process_title(value);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_exepath(IntPtr buffer, ref IntPtr size);

		public static string ExecutablePath {
			get {
				int size = 512;
				var buffer = Marshal.AllocHGlobal(size);
				var sizePtr = new IntPtr(size);
				uv_exepath(buffer, ref sizePtr);

				string ret = Marshal.PtrToStringAnsi(buffer);
				Marshal.FreeHGlobal(buffer);
				return ret;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_spawn(IntPtr loop, IntPtr handle, uv_process_options_t options);

		uv_process_options_t process_options;

		internal Process(Loop loop, ProcessOptions options, Action<Process> exitCallback)
			: base(loop, UvHandleType.UV_PROCESS)
		{
			process_options = new uv_process_options_t(options, (exit_status, term_status) => {
				ExitCode = exit_status;
				TermSignal = term_status;
				exitCallback(this);
				Close();
			});
		}

		public static Process Spawn(ProcessOptions options, Action<Process> exitCallback)
		{
			return Spawn(Loop.Default, options, exitCallback);
		}

		public static Process Spawn(Loop loop, ProcessOptions options, Action<Process> exitCallback)
		{
			var process = new Process(loop, options, exitCallback);
			int r = uv_spawn(loop.NativeHandle, process.NativeHandle, process.process_options);
			Ensure.Success(r, loop);
			return process;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_process_kill(IntPtr handle, int signum);

		public void Kill(int signum)
		{
			Ensure.Success(uv_process_kill(NativeHandle, signum), Loop);
		}

		public void Kill(Signum signum)
		{
			Kill((int)signum);
		}
	}
}

