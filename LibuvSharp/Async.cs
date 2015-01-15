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
		internal static extern int uv_async_init(LoopSafeHandle loop, IntPtr handle, IntPtr callback);

		public Async()
			: this(Loop.Constructor)
		{
		}

		async_cb cb;
		public Async(Loop loop)
			: base(loop, HandleType.UV_ASYNC)
		{
			cb = (_, status) => {
				OnCallback();
			};

			uv_async_init(loop.NativeHandle, NativeHandle, Marshal.GetFunctionPointerForDelegate(cb));
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

	public class AsyncWatcher<T> : IHandle, IDisposable
	{
		Async async;
		Queue<T> queue = new Queue<T>();

		public AsyncWatcher()
			: this(Loop.Constructor)
		{
		}

		public AsyncWatcher(Loop loop)
		{
			async = new Async(loop);
			async.Callback += (_) => {
				Queue<T> tmp;
				lock (queue) {
					tmp = new Queue<T>();
					while (queue.Count > 0) {
						tmp.Enqueue(queue.Dequeue());
					}
				}
				while (tmp.Count > 0) {
					OnCallback(tmp.Dequeue());
				}
			};
		}

		public void Ref()
		{
			async.Ref();
		}

		public void Unref()
		{
			async.Unref();
		}

		public bool IsClosed {
			get {
				return async.IsClosed;
			}
		}

		public void Close(Action callback)
		{
			async.Close(callback);
		}

		public void Dispose()
		{
			this.Close();
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

