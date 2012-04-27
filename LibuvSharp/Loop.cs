using System;
using System.Runtime.InteropServices;

namespace Libuv
{
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

		internal ByteBuffer buffer = new ByteBuffer();

		static Loop @default = new Loop(uv_default_loop());
		public static Loop Default {
			get {
				return @default;
			}
		}

		Lazy<Dns> dns;
		public Dns Dns {
			get {
				return dns.Value;
			}
		}

		internal IntPtr Handle { get; set; }

		AsyncCallback callback;

		internal Loop(IntPtr handle)
		{
			Handle = handle;

			dns = new Lazy<Libuv.Dns>(() => new Dns(this));
			callback = new AsyncCallback(this);

			// this fixes a strange bug, where you can't send async
			// stuff from other threads
			Sync (() => { });

			Unref(); // ignore our allocated resources
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
			var before = new CAction<IntPtr>((ptr) => callback());
			var cafter = new CAction<IntPtr>((ptr) => {
				pr.Dispose();
				if (after != null) {
					after();
				}
			});

			int r = uv_queue_work(Handle, pr.Handle, before.Callback, cafter.Callback);
			UV.EnsureSuccess(r);
		}

		public void Sync(Action cb)
		{
			callback.Send(cb);
		}

		public void Sync(System.Collections.Generic.IEnumerable<Action> callbacks)
		{
			callback.Send(callbacks);
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

			if (callback != null) {
				callback = null;
				callback.Close();
			}

			if (Handle != Default.Handle) {
				uv_loop_delete(Handle);
			}

			if (buffer != null) {
				buffer.Dispose();
				buffer = null;
			}
		}
	}
}

