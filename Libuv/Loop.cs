using System;
using System.Runtime.InteropServices;

namespace Libuv
{
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

	public class Loop : IDisposable
	{
		[DllImport("uv")]
		static extern IntPtr uv_default_loop();

		[DllImport("uv")]
		static extern IntPtr uv_loop_new();

		[DllImport("uv")]
		static extern void uv_loop_delete(IntPtr ptr);

		[DllImport("uv")]
		static extern void uv_run(IntPtr loop);

		[DllImport("uv")]
		static extern void uv_run_once(IntPtr loop);

		[DllImport("uv")]
		static extern void uv_ref(IntPtr loop);

		[DllImport("uv")]
		static extern void uv_unref(IntPtr loop);

		[DllImport("uv")]
		static extern void uv_update_time(IntPtr loop);

		[DllImport("uv")]
		static extern long uv_now(IntPtr loop);

		static Loop @default = new Loop(uv_default_loop());
		public static Loop Default {
			get {
				return @default;
			}
		}

		public Dns Dns { get; protected set; }

		internal IntPtr Handle { get; set; }

		internal Loop(IntPtr handle)
		{
			Handle = handle;
			Dns = new Dns(this);
		}

		public Loop()
			: this(uv_loop_new())
		{
		}

		public void Run()
		{
			uv_run(Handle);
		}

		public void RunOnce()
		{
			uv_run_once(Handle);
		}

		public void Ref()
		{
			uv_ref(Handle);
		}

		public void Unref()
		{
			uv_unref(Handle);
		}

		public void UpdateTime()
		{
			uv_update_time(Handle);
		}

		public long Now {
			get {
				return uv_now(Handle);
			}
		}

		[DllImport("uv")]
		static extern int uv_queue_work(IntPtr loop, IntPtr req, Action<IntPtr> work_cb, Action<IntPtr> after_work_cb);

		public void QueueWork(Action callback)
		{
			QueueWork(callback, () => { });
		}

		public void QueueWork(Action callback, Action after)
		{
			var pr = new PermaRequest(UV.Sizeof(UvRequestType.Work));
			var permaCallback = new PermaCallback<IntPtr>((ptr) => callback());
			var permaAfter = new PermaCallback<IntPtr>((ptr) => {
				pr.Dispose();
				if (after != null) {
					after();
				}
			});

			int r = uv_queue_work(Handle, pr.Handle, permaCallback.Callback, permaAfter.Callback);
			UV.EnsureSuccess(r);
		}

		~Loop()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		public void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			Dns.Dispose();

			if (Handle != Default.Handle) {
				uv_loop_delete(Handle);
			}
		}
	}
}

