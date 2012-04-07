using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	internal class PermaCallback : IDisposable
	{
		public Action Callback { get; protected set; }

		GCHandle GCHandle { get; set; }
		Action cb;

		public PermaCallback(Action callback)
		{
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback()
		{
			cb();
			Dispose();
		}

		~PermaCallback()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}
	}

	internal class PermaCallback<T> : IDisposable
	{
		public Action<T> Callback { get; protected set; }

		GCHandle GCHandle { get; set; }
		Action<T> cb;

		public PermaCallback(Action<T> callback)
		{
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T arg1)
		{
			cb(arg1);
			Dispose();
		}

		~PermaCallback()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}
	}

	internal class PermaCallback<T1, T2> : IDisposable
	{
		public Action<T1, T2> Callback { get; protected set; }

		GCHandle GCHandle { get; set; }
		Action<T1, T2> cb;

		public PermaCallback(Action<T1, T2> callback)
		{
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T1 arg1, T2 arg2)
		{
			cb(arg1, arg2);
			Dispose();
		}

		~PermaCallback()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}
	}

	internal class PermaCallback<T1, T2, T3> : IDisposable
	{
		public Action<T1, T2, T3> Callback { get; protected set; }

		GCHandle GCHandle { get; set; }
		Action<T1, T2, T3> cb;

		public PermaCallback(Action<T1, T2, T3> callback)
		{
			GCHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T1 arg1, T2 arg2, T3 arg3)
		{
			cb(arg1, arg2, arg3);
			Dispose();
		}

		~PermaCallback()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			GCHandle.Free();
		}
	}
}

