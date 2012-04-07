using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Async : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_async_init(IntPtr loop, IntPtr handle, Action<IntPtr, int> callback);

		GCHandle GCHandle { get; set; }

		public Async()
			: this(null as Action)
		{
		}

		public Async(Action cb)
			: this(Loop.Default, cb)
		{
		}

		public Async(Loop loop)
			: this(loop, null)
		{
		}

		public Async(Loop loop, Action cb)
			: base(loop, UvHandleType.Async)
		{
			Action<IntPtr, int> callback = (_, status) => {
				OnCallback();
			};

			GCHandle = GCHandle.Alloc(callback, GCHandleType.Pinned);
			uv_async_init(loop.Handle, handle, callback);

			if (cb != null) {
				Callback += cb;
			}
		}

		[DllImport("uv")]
		internal static extern int uv_async_send(IntPtr handle);

		public void Send()
		{
			uv_async_send(handle);
		}

		public void OnCallback()
		{
			if (Callback != null) {
				Callback();
			}
		}

		public event Action Callback;
	}

	public class AsyncWatcher<T>
	{
		Async async;
		Queue<T> queue = new Queue<T>();

		public AsyncWatcher()
			: this(null as Action<T>)
		{
		}

		public AsyncWatcher(Action<T> callback)
			: this(Loop.Default, callback)
		{
		}

		public AsyncWatcher(Loop loop)
			: this(loop, null)
		{
		}

		public AsyncWatcher(Loop loop, Action<T> callback)
		{
			async = new Async(loop, () => {
				Queue<T> tmp;
				lock (queue) {
					tmp = new Queue<T>(queue);
				}
				while (tmp.Count > 0) {
					OnCallback(tmp.Dequeue());
				}
			});

			if (callback != null) {
				Callback += callback;
			}
		}

		public void Send(T item)
		{
			lock (queue) {
				queue.Enqueue(item);
			}
			async.Send();
		}

		public void OnCallback(T item)
		{
			if (Callback != null) {
				Callback(item);
			}
		}

		public event Action<T> Callback;

		public void Close()
		{
			async.Close();
		}

		public void Close(Action callback)
		{
			async.Close(callback);
		}
	}

	public class AsyncCallback : AsyncWatcher<Action>
	{
		public AsyncCallback(Loop loop)
			: base(loop)
		{
			Callback += (callback) => callback();
		}
	}
}

