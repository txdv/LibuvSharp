using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Listener<TStream> : Handle, IListener<TStream> where TStream : class, IUVStream
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
		void listen_callback(IntPtr handle, int status)
		{
			OnIncommingStream();
		}

		protected abstract UVStream Create();

		public void Listen(int backlog)
		{
			int r = uv_listen(NativeHandle, backlog, listen_cb);
			Ensure.Success(r, Loop);
		}

		public void Listen()
		{
			Listen(DefaultBacklog);
		}

		public TStream AcceptStream()
		{
			var stream = Create();
			uv_accept(NativeHandle, stream.NativeHandle);
			return stream as TStream;
		}

		private void OnIncommingStream()
		{
			if (IncommingStream != null) {
				IncommingStream();
			}
		}

		public event Action IncommingStream;
	}
}

