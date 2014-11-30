using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	enum uv_run_mode : int
	{
		UV_RUN_DEFAULT = 0,
		UV_RUN_ONCE,
		UV_RUN_NOWAIT
	};

	public partial class Loop : IDisposable
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern LoopSafeHandle uv_default_loop();

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern LoopSafeHandle uv_loop_new();

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_run(LoopSafeHandle loop, uv_run_mode mode);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_update_time(LoopSafeHandle loop);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern ulong uv_now(LoopSafeHandle loop);

		static Loop @default;

		public static Loop Default {
			get {
				if (@default == null) {
					@default = new Loop(uv_default_loop(), new CopyingByteBufferAllocator());
				}
				return @default;
			}
		}

		[ThreadStatic]
		private static Loop currentLoop;

		/// <summary>
		/// Returns Default Loop value when creating LibuvSharp objects.
		/// </summary>
		/// <value>A loop.</value>
		internal static Loop Constructor {
			get {
				return Loop.Current ?? Loop.Default;
			}
		}

		public LoopSafeHandle NativeHandle { get; protected set; }

		public ByteBufferAllocatorBase ByteBufferAllocator { get; protected set; }

		Async async;
		AsyncCallback callback;

		internal Loop(LoopSafeHandle handle, ByteBufferAllocatorBase allocator)
		{
			NativeHandle = handle;
			ByteBufferAllocator = allocator;

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
			: this(new CopyingByteBufferAllocator())
		{
		}

		public Loop(ByteBufferAllocatorBase allocator)
			: this(uv_loop_new(), allocator)
		{
		}

		unsafe uv_loop_t* loop_t {
			get {
				return (uv_loop_t *)NativeHandle.Handle;
			}
		}

		unsafe public uint ActiveHandlesCount {
			get {
				return loop_t->active_handlers;
			}
		}

		unsafe public IntPtr Data {
			get {
				return loop_t->data;
			}
			set {
				loop_t->data = value;
			}
		}

		public bool IsRunning { get; private set; }

		private bool RunGuard(Action action)
		{
			if (IsRunning) {
				return false;
			}

			// save the value, restore it aftwards
			var tmp = currentLoop;

			IsRunning = true;
			currentLoop = this;

			if (action != null) {
				action();
			}

			IsRunning = false;
			currentLoop = tmp;

			return true;
		}

		private bool RunGuard(Action context, Func<bool> func)
		{
			if (!RunGuard(context)) {
				return false;
			}
			return func();
		}

		public bool Run()
		{
			return RunGuard(() => uv_run(NativeHandle, uv_run_mode.UV_RUN_DEFAULT));
		}

		public bool Run(Action context)
		{
			return RunGuard(context, Run);
		}

		public bool RunOnce()
		{
			return RunGuard(() => uv_run(NativeHandle, uv_run_mode.UV_RUN_ONCE));
		}

		public bool RunOnce(Action context)
		{
			return RunGuard(context, RunOnce);
		}

		public bool RunAsync()
		{
			return RunGuard(() => uv_run(NativeHandle, uv_run_mode.UV_RUN_NOWAIT));
		}

		public bool RunAsync(Action context)
		{
			return RunGuard(context, RunAsync);
		}

		public void UpdateTime()
		{
			uv_update_time(NativeHandle);
		}

		public ulong Now {
			get {
				return uv_now(NativeHandle);
			}
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
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				if (ByteBufferAllocator != null) {
					ByteBufferAllocator.Dispose();
					ByteBufferAllocator = null;
				}
			}

			if (NativeHandle != Default.NativeHandle) {
				NativeHandle.Dispose();
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void walk_cb(IntPtr handle, IntPtr arg);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_walk(LoopSafeHandle loop, walk_cb cb, IntPtr arg);

		static walk_cb walk_callback = WalkCallback;
		static void WalkCallback(IntPtr handle, IntPtr arg)
		{
			var gchandle = GCHandle.FromIntPtr(arg);
			(gchandle.Target as Action<IntPtr>)(handle);
		}

		public void Walk(Action<IntPtr> callback)
		{
			var gchandle = GCHandle.Alloc(callback, GCHandleType.Normal);
			uv_walk(NativeHandle, walk_callback, GCHandle.ToIntPtr(gchandle));
			gchandle.Free();
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

		public int RefCount { get; private set; }

		public void Ref() {
			if (RefCount == 0) {
				async.Ref();
			}
			RefCount++;
		}

		public void Unref() {
			if (RefCount <= 0) {
				return;
			}
			if (RefCount == 1) {
				async.Unref();
			}
			RefCount--;
		}

		LoopBackend loopBackend;
		public LoopBackend Backend {
			get {
				if (loopBackend == null) {
					loopBackend = new LoopBackend(this);
				}
				return loopBackend;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr uv_stop(LoopSafeHandle loop);

		public void Stop()
		{
			uv_stop(NativeHandle);
		}
	}
}

