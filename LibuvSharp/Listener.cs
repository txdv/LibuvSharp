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
			Invoke(NativeMethods.uv_listen, backlog, listen_cb);
		}

		public void Listen()
		{
			Listen(DefaultBacklog);
		}

		public TStream Accept()
		{
			var stream = Create();
			try {
				Invoke(NativeMethods.uv_accept, stream.NativeHandle);
			} catch (Exception) {
				stream.Dispose();
				throw;
			}
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

