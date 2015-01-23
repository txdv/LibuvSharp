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

		uv_fs_event_cb uv_fs_event;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_event_init(IntPtr loop, IntPtr handle);

		public FileSystemWatcher()
			: this(Loop.Constructor)
		{
		}

		public FileSystemWatcher(Loop loop)
			: base(loop, HandleType.UV_FS_EVENT)
		{
			uv_fs_event = fs_event;
			int r = uv_fs_event_init(loop.NativeHandle, NativeHandle);
			Ensure.Success(r);
		}

		public void Start(string path)
		{
			Start(path, FileSystemEventFlags.Default);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_event_start(IntPtr handle, uv_fs_event_cb callback, string filename, int flags);

		public void Start(string path, FileSystemEventFlags flags)
		{
			uv_fs_event = fs_event;
			int r = uv_fs_event_start(NativeHandle, uv_fs_event, path, (int)flags);
			Ensure.Success(r);
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

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_getpath(IntPtr handle, IntPtr buf, ref IntPtr len);

		public string Path {
			get {
				return UV.ToString(4096, (buffer, length) => uv_fs_event_getpath(NativeHandle, buffer, ref length)).TrimEnd('\0');
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_fs_event_stop(IntPtr handle);

		public void Stop()
		{
			int r = uv_fs_event_stop(NativeHandle);
			Ensure.Success(r);
		}

	}
}

