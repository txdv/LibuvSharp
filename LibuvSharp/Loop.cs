using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Loop : IDisposable
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr uv_default_loop();

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr uv_loop_new();

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_loop_delete(IntPtr ptr);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_run(IntPtr loop);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_run_once(IntPtr loop);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_update_time(IntPtr loop);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern long uv_now(IntPtr loop);

		internal ByteBuffer buffer = new ByteBuffer();


		static Lazy<Loop> @default = new Lazy<Loop>(() => new Loop(uv_default_loop()));
		public static Loop Default {
			get {
				return @default.Value;
			}
		}

		internal IntPtr Handle { get; set; }

		Async async;
		AsyncCallback callback;

		internal Loop(IntPtr handle)
		{
			Handle = handle;

			var that = this;
			callback = new AsyncCallback(this);
			async = new Async(this);

			// this fixes a strange bug, where you can't send async
			// stuff from other threads
			Sync(() => { });
			async.Send();
			RunOnce();

			// ignore our allocated resources
			async.Unref();
			callback.Unref();
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

		public void RunAsync()
		{
			async.Send();
			RunOnce();
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
			var pr = new PermaRequest(UV.Sizeof(UvRequestType.UV_WORK));
			var before = new CAction<IntPtr>((ptr) => callback());
			var cafter = new CAction<IntPtr>((ptr) => {
				pr.Dispose();
				if (after != null) {
					after();
				}
			});

			int r = uv_queue_work(Handle, pr.Handle, before.Callback, cafter.Callback);
			Ensure.Success(r, this);
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

		protected void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			if (Handle != Default.Handle) {
				// uv_loop_delete(Handle);
			}

			if (buffer != null) {
				buffer.Dispose();
				buffer = null;
			}
		}

		delegate void walk_cb(IntPtr handle, IntPtr arg);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_walk(IntPtr loop, Action<IntPtr, IntPtr> cb, IntPtr arg);

		public void Walk(Action<IntPtr> callback)
		{
			var cb = new CAction<IntPtr, IntPtr>((handle, arg) => callback(handle));
			uv_walk(Handle, cb.Callback, IntPtr.Zero);
		}

		public IntPtr[] Handles {
			get {
				var list = new List<IntPtr>();
				Walk((handle) => list.Add(handle));
				return list.ToArray();
			}
		}

		internal Dictionary<IntPtr, Handle> handles = new Dictionary<IntPtr, Handle>();

		public Handle GetHandle(IntPtr ptr)
		{
			Handle handle;
			if (handles.TryGetValue(ptr, out handle)) {
				return handle;
			} else {
				return null;
			}
		}

		public Handle[] ActiveHandles {
			get {
				var tmp = Handles;
				Handle[] handles = new Handle[tmp.Length];
				for (var i = 0; i < tmp.Length; i++) {
					handles[i] = GetHandle(tmp[i]);
				}
				return handles;
			}
		}
	}
}

