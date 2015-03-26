using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public enum FileSystemEventFlags
	{
		Default = 0,
		WatchEntry = 1,
		Stat = 2,
		Recursive = 3
	}

	public enum FileSystemEvent
	{
		Rename = 1,
		Change = 2
	}

	public class FileSystemWatcher : Handle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void uv_fs_event_cb(IntPtr handle, string filename, int events, int status);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_init(IntPtr loop, IntPtr handle);

		static uv_fs_event_cb fs_event_callback;
		static FileSystemWatcher()
		{
			fs_event_callback = fs_event;
		}

		public FileSystemWatcher()
			: this(Loop.Constructor)
		{
		}

		public FileSystemWatcher(Loop loop)
			: base(loop, HandleType.UV_FS_EVENT, uv_fs_event_init)
		{
		}

		public void Start(string path)
		{
			Start(path, FileSystemEventFlags.Default);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_start(IntPtr handle, uv_fs_event_cb callback, string filename, int flags);

		public void Start(string path, FileSystemEventFlags flags)
		{
			Invoke(uv_fs_event_start, fs_event_callback, path, (int)flags);
		}

		static void fs_event(IntPtr handlePointer, string filename, int events, int status)
		{
			var handle = FromIntPtr<FileSystemWatcher>(handlePointer);
			if (status != 0) {
				handle.Close();
			} else {
				handle.OnChange(filename, (FileSystemEvent)events);
			}

		}

		public event Action<string, FileSystemEvent> Change;

		void OnChange(string filename, FileSystemEvent @event)
		{
			if (Change != null) {
				Change(filename, @event);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_getpath(IntPtr handle, IntPtr buf, ref IntPtr len);

		public string Path {
			get {
				CheckDisposed();

				return UV.ToString(4096, (IntPtr buffer, ref IntPtr length) => uv_fs_event_getpath(NativeHandle, buffer, ref length));
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_stop(IntPtr handle);

		public void Stop()
		{
			Invoke(uv_fs_event_stop);
		}
	}
}

