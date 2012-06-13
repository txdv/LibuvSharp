using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class FileSystemEvent : Handle
	{
		delegate void uv_fs_event_cb(IntPtr handle, string filename, int events, int status);

		[DllImport("uv")]
		private static extern int uv_fs_event_init(IntPtr loop, IntPtr handle, string filename, uv_fs_event_cb callback, int flags);

		uv_fs_event_cb uv_fs_event;

		public string Path { get; protected set; }

		public FileSystemEvent(string path)
			: this(Loop.Default, path)
		{
			Path = path;
		}

		public FileSystemEvent(Loop loop, string path)
			: base(loop, UvHandleType.UV_FS_EVENT)
		{
			uv_fs_event = fs_event;
			int r = uv_fs_event_init(loop.Handle, handle, path, uv_fs_event, 0);
			Ensure.Success(r, loop);
		}

		void fs_event(IntPtr handle, string filename, int events, int status)
		{
			if (status != 0) {
				Close();
			} else {
				OnChanged((PollEvent)events);
			}
		}

		public event Action<PollEvent> Changed;

		protected void OnChanged(PollEvent @event)
		{
			if (Changed != null) {
				Changed(@event);
			}
		}
	}
}

