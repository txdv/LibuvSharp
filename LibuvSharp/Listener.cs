using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Listener<TStream> : Handle, IListener<TStream> where TStream : class
	{
		internal Listener(Loop loop, HandleType type)
			: base(loop, type)
		{
			DefaultBacklog = 128;
			listen_cb = listen_callback;
		}

		internal Listener(Loop loop, HandleType type, Func<IntPtr, IntPtr, int> constructor)
			: this(loop, type)
		{
			Construct(constructor);
		}

		internal Listener(Loop loop, HandleType handleType, Func<IntPtr, IntPtr, int, int> constructor, int arg1)
			: this(loop, handleType)
		{
			Construct(constructor, arg1);
		}

		public int DefaultBacklog { get; set; }

		callback listen_cb;
		void listen_callback(IntPtr handle, int status)
		{
			OnConnection();
		}

		protected abstract UVStream Create();

		public void Listen(int backlog)
		{
			CheckDisposed();

			int r = NativeMethods.uv_listen(NativeHandle, backlog, listen_cb);
			Ensure.Success(r);
		}

		public void Listen()
		{
			Listen(DefaultBacklog);
		}

		public TStream Accept()
		{
			var stream = Create();
			int r = NativeMethods.uv_accept(NativeHandle, stream.NativeHandle);
			Ensure.Success(r);
			return stream as TStream;
		}

		void OnConnection()
		{
			if (Connection != null) {
				Connection();
			}
		}

		public event Action Connection;
	}
}

