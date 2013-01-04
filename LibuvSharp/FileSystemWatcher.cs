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
		delegate void uv_fs_event_cb(IntPtr handle, string filename, int events, int status);

		[DllImport("uv")]
		private static extern int uv_fs_event_init(IntPtr loop, IntPtr handle, string filename, uv_fs_event_cb callback, int flags);

		uv_fs_event_cb uv_fs_event;

		public string Path { get; protected set; }

		public FileSystemWatcher(string path)
			: this(path, FileSystemEventFlags.Default)
		{
		}

		public FileSystemWatcher(string path, FileSystemEventFlags flags)
			: this(Loop.Default, path, flags)
		{
			Path = path;
		}

		public FileSystemWatcher(Loop loop, string path)
			: this(loop, path, FileSystemEventFlags.Default)
		{
		}

		public FileSystemWatcher(Loop loop, string path, FileSystemEventFlags flags)
			: base(loop, HandleType.UV_FS_EVENT)
		{
			uv_fs_event = fs_event;
			int r = uv_fs_event_init(loop.NativeHandle, NativeHandle, path, uv_fs_event, (int)flags);
			Ensure.Success(r, loop);
		}

		void fs_event(IntPtr handle, string filename, int events, int status)
		{
			if (status != 0) {
				Close();
			} else {
				OnChange(filename, (FileSystemEvent)events);
			}
		}

		public event Action<string, FileSystemEvent> Change;

		void OnChange(string filename, FileSystemEvent @event)
		{
			if (Change != null) {
				Change(filename, @event);
			}
		}
	}
}

