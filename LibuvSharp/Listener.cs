using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Listener : Handle, IListener
	{
		internal Listener(Loop loop, HandleType type)
			: base(loop, type)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_listen(IntPtr stream, int backlog, callback callback);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_accept(IntPtr server, IntPtr client);

		public int DefaultBacklog { get; set; }

		callback listen_cb;
		void listen_callback(IntPtr req, int status)
		{
			UVStream stream = Create();
			uv_accept(req, stream.NativeHandle);
			listenCallback(stream);
		}

		protected abstract UVStream Create();

		Action<UVStream> listenCallback;

		public void Listen(int backlog, Action<UVStream> callback)
		{
			Ensure.ArgumentNotNull(callback, "callback");
			listenCallback = callback;
			int r = uv_listen(NativeHandle, backlog, listen_cb);
			Ensure.Success(r, Loop);
		}

		public void Listen(Action<UVStream> callback)
		{
			Listen(DefaultBacklog, callback);
		}
	}
}

