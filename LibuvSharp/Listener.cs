using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Listener : Handle, IListener
	{
		internal Listener(Loop loop, UvHandleType type)
			: base(loop, type)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_listen(IntPtr stream, int backlog, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_accept(IntPtr server, IntPtr client);

		public int DefaultBacklog { get; set; }

		callback listen_cb;
		void listen_callback(IntPtr req, int status)
		{
			UVStream stream = Create();
			uv_accept(req, stream.NativeHandle);
			OnListen(stream);
		}

		protected abstract UVStream Create();

		protected event Action<UVStream> OnListen;

		public void Listen(int backlog, Action<UVStream> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");
			OnListen += callback;
			int r = uv_listen(NativeHandle, backlog, listen_cb);
			Ensure.Success(r, Loop);
		}

		public void Listen(Action<UVStream> callback)
		{
			Listen(DefaultBacklog, callback);
		}
	}
}

