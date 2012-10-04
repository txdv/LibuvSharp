using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Async : Handle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void async_cb(IntPtr handle, int status);

		[DllImport("uv", CallingConvention=CallingConvention.Cdecl)]
		internal static extern int uv_async_init(IntPtr loop, IntPtr handle, IntPtr callback);

		GCHandle GCHandle { get; set; }

		public Async()
			: this(null as Action<Async>)
		{
		}

		public Async(Action<Async> callback)
			: this(Loop.Default, callback)
		{
		}

		public Async(Loop loop)
			: this(loop, null)
		{
		}

		async_cb cb;
		public Async(Loop loop, Action<Async> callback)
			: base(loop, HandleType.UV_ASYNC)
		{
			cb = (_, status) => {
				OnCallback();
			};

			uv_async_init(loop.NativeHandle, NativeHandle, Marshal.GetFunctionPointerForDelegate(cb));

			Callback += callback;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_async_send(IntPtr handle);

		public void Send()
		{
			uv_async_send(NativeHandle);
		}

		public event Action<Async> Callback;

		public void OnCallback()
		{
			if (Callback != null) {
				Callback(this);
			}
		}
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
			async = new Async(loop, (_) => {
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

		public void Unref()
		{
			async.Unref();
		}

		public void Ref()
		{
			async.Ref();
		}

		public void Send(T item)
		{
			lock (queue) {
				queue.Enqueue(item);
			}
			async.Send();
		}

		public void Send(IEnumerable<T> data) {
			Ensure.ArgumentNotNull(data, "data");

			lock (queue) {
				foreach (var dataitem in data) {
					queue.Enqueue(dataitem);
				}
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

