using System;
using System.Collections.Generic;
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

	public class ProcessOptions
	{
		public string File { get; set; }
		public string[] Arguments { get; set; }
		public string[] Environment { get; set; }
		public string CurrentWorkingDirectory { get; set; }
		public bool WindowsVerbatimArguments { get; set; }
		public bool Detached { get; set; }
		public int? UID { get; set; }
		public int? GID { get; set ;}

		public ICollection<UVStream> Streams { get; set; }
	}

	unsafe public class Process : Handle
	{
		public long ExitCode { get; private set; }
		public int TermSignal { get; private set; }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_get_process_title(IntPtr buffer, IntPtr size);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_set_process_title(string title);

		public static string Title {
			get {
				return UV.ToString(4096, uv_get_process_title);
			}
			set {
				uv_set_process_title(value);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_exepath(IntPtr buffer, ref IntPtr size);

		public static string ExecutablePath {
			get {
				return UV.ToString(4096, uv_exepath);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_spawn(IntPtr loop, IntPtr handle, ref uv_process_options_t options);

		uv_process_options_t process_options;
		Action<Process> exitCallback;

		internal Process(Loop loop, ProcessOptions options, Action<Process> exitCallback)
			: base(loop, HandleType.UV_PROCESS)
		{
			this.exitCallback = exitCallback;
			process_options = new uv_process_options_t(this, options);
		}

		internal void OnExit(long exit_status, int term_status)
		{
			process_options.Dispose();
			ExitCode = exit_status;
			TermSignal = term_status;
			if (exitCallback != null) {
				exitCallback(this);
			}
			Close();
		}

		uv_process_t* process {
			get {
				CheckDisposed();

				return (uv_process_t*)(NativeHandle.ToInt32() + Handle.Size(HandleType.UV_HANDLE));
			}
		}

		public int ID {
			get {
				return process->pid;
			}
		}

		public static Process Spawn(ProcessOptions options)
		{
			return Spawn(options, null);
		}

		public static Process Spawn(ProcessOptions options, Action<Process> exitCallback)
		{
			return Spawn(Loop.Constructor, options, exitCallback);
		}

		public static Process Spawn(Loop loop, ProcessOptions options)
		{
			return Spawn(loop, options, null);
		}

		public static Process Spawn(Loop loop, ProcessOptions options, Action<Process> exitCallback)
		{
			var process = new Process(loop, options, exitCallback);
			int r = uv_spawn(loop.NativeHandle, process.NativeHandle, ref process.process_options);
			Ensure.Success(r);
			return process;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_process_kill(IntPtr handle, int signum);

		public void Kill(int signum)
		{
			Invoke(uv_process_kill, signum);
		}

		public void Kill(Signum signum)
		{
			Kill((int)signum);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_disable_stdio_inheritance();

		public static void DisableStdioInheritance()
		{
			uv_disable_stdio_inheritance();
		}
	}
}

