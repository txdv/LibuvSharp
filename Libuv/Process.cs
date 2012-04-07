using System;
using System.Runtime.InteropServices;

namespace Libuv
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

	[StructLayout(LayoutKind.Sequential)]
	unsafe internal struct uv_process_options_t : IDisposable
	{
		// fields

		public Action<IntPtr, int, int> exit_cb;
		public IntPtr file;
		public IntPtr args;
		public IntPtr env;
		public IntPtr cwd;

		public int windows_verbatim_arguments;

		public IntPtr stdin_stream;
		public IntPtr stdout_stream;
		public IntPtr stderr_stream;


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

			windows_verbatim_arguments = (options.WindowsVerbatimArguments ? 1 : 0);

			stdin_stream = IntPtr.Zero;
			stdout_stream = IntPtr.Zero;
			stderr_stream = IntPtr.Zero;

			exit_cb = new PermaCallback<IntPtr, int, int>((handle, exit_status, term_signal) => {
				exitCallback(exit_status, term_signal);
			}).Callback;
		}

		public void Dispose()
		{
			if (file != IntPtr.Zero) {
				Marshal.FreeHGlobal(file);
			}

			free(args);
			free(env);

			if (cwd != IntPtr.Zero) {
				Marshal.FreeHGlobal(file);
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

		static void free(IntPtr ptr)
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
			Marshal.FreeHGlobal(p);
		}
	}

	public class ProcessOptions
	{
		public string File { get; set; }
		public string[] Arguments { get; set; }
		public string[] Environment { get; set; }
		public string CurrentWorkingDirectory { get; set; }
		public bool WindowsVerbatimArguments { get; set; }
	}

	public class Process : Handle
	{
		[DllImport("uv")]
		internal static extern uv_err_t uv_get_process_title(IntPtr buffer, IntPtr size);

		[DllImport("uv")]
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

		[DllImport("uv")]
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


		public Pipe Stdin { get; protected set; }
		public Pipe Stdout { get; protected set; }
		public Pipe Stderr { get; protected set; }

		[DllImport("uv")]
		internal static extern int uv_spawn(IntPtr loop, IntPtr handle, uv_process_options_t options);

		internal Process(Loop loop)
			: base(loop, UvHandleType.Process)
		{
		}

		public static Process Spawn(ProcessOptions options, Action<Process, int, int> exitCallback)
		{
			return Spawn(Loop.Default, options, exitCallback);
		}

		public static Process Spawn(Loop loop, ProcessOptions options, Action<Process, int, int> exitCallback)
		{
			Process process = new Process(loop);

			process.Stdin = new Pipe(loop, false);
			process.Stdout = new Pipe(loop, false);
			process.Stderr = new Pipe(loop, false);

			uv_process_options_t options_t = new uv_process_options_t(options, (exit_status, term_status) => {
				options_t.Dispose();
				exitCallback(process, exit_status, term_status);
			});

			options_t.stdin_stream = process.Stdin.handle;
			options_t.stdout_stream = process.Stdout.handle;
			options_t.stderr_stream = process.Stderr.handle;

			int r = uv_spawn(loop.Handle, process.handle, options_t);
			UV.EnsureSuccess(r);

			return process;
		}

		[DllImport("uv")]
		internal static extern int uv_process_kill(IntPtr handle, int signum);

		public void Kill(int signum)
		{
			UV.EnsureSuccess(uv_process_kill(handle, signum));
		}

		public void Kill(Signum signum)
		{
			Kill((int)signum);
		}

	}
}

