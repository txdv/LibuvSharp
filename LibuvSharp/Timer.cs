using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Timer : Handle
	{
		[DllImport("uv")]
		internal static extern int uv_timer_init(IntPtr loop, IntPtr timer);

		[DllImport("uv")]
		internal static extern int uv_timer_start(IntPtr timer, IntPtr callback, long timeout, long repeat);

		[DllImport("uv")]
		internal static extern int uv_timer_stop(IntPtr timer);

		[DllImport("uv")]
		internal static extern int uv_timer_again(IntPtr timer);

		[DllImport("uv")]
		internal static extern void uv_timer_set_repeat(IntPtr timer, long repeat);

		[DllImport("uv")]
		internal static extern long uv_timer_get_repeat(IntPtr timer);

		private Action<IntPtr, int> cb;

		public Timer()
			: this(Loop.Default)
		{
		}

		public Timer(Loop loop)
			: base(loop, UvHandleType.UV_TIMER)
		{
			uv_timer_init(loop.Handle, handle);
		}

		public long LongRepeat {
			get {
				return uv_timer_get_repeat(handle);
			}
			set {
				uv_timer_set_repeat(handle, value);
			}
		}

		public TimeSpan Repeat {
			get {
				return TimeSpan.FromMilliseconds(LongRepeat);
			}
			set {
				LongRepeat = (long)value.TotalMilliseconds;
			}
		}

		public bool Running { get; private set; }

		internal void Start(long timeout, long repeat, IntPtr callback)
		{
			if (Running) {
				Stop();
			}
			Running = true;
			LongRepeat = repeat;
			uv_timer_start(handle, callback, timeout, repeat);
		}

		public void Start(long timeout, long repeat, Action<Timer, int> callback)
		{
			cb = delegate (IntPtr h, int status) {
				callback(this, status);
			};

			Start(timeout, repeat, Marshal.GetFunctionPointerForDelegate(cb));
		}
		public void Start(long repeat, Action<Timer, int> callback)
		{
			Start(0, repeat, callback);
		}
		public void Start(TimeSpan timeout, TimeSpan repeat, Action<Timer, int> callback)
		{
			Start((long)timeout.TotalMilliseconds, (long)repeat.TotalMilliseconds, callback);
		}
		public void Start(TimeSpan repeat, Action<Timer, int> callback)
		{
			Start(TimeSpan.Zero, repeat, callback);
		}

		public void Start(long timeout, long repeat, Action<int> callback)
		{
			cb = delegate (IntPtr h, int status) {
				callback(status);
			};

			Start(timeout, repeat, Marshal.GetFunctionPointerForDelegate(cb));
		}
		public void Start(long repeat, Action<int> callback)
		{
			Start(0, repeat, callback);
		}
		public void Start(TimeSpan timeout, TimeSpan repeat, Action<int> callback)
		{
			Start((long)timeout.TotalMilliseconds, (long)repeat.TotalMilliseconds, callback);
		}
		public void Start(TimeSpan repeat, Action<int> callback)
		{
			Start(TimeSpan.Zero, repeat, callback);
		}

		public void Start(long timeout, long repeat, Action callback)
		{
			cb = delegate (IntPtr h, int status) {
				callback();
			};
			Start(timeout, repeat, Marshal.GetFunctionPointerForDelegate(cb));
		}
		public void Start(long repeat, Action callback)
		{
			Start(0, repeat, callback);
		}
		public void Start(TimeSpan timeout, TimeSpan repeat, Action callback)
		{
			Start((long)timeout.TotalMilliseconds, (long)repeat.TotalMilliseconds, callback);
		}
		public void Start(TimeSpan repeat, Action callback)
		{
			Start(TimeSpan.Zero, repeat, callback);
		}

		public void Stop()
		{
			if (Running) {
				uv_timer_stop(handle);
			}
			Running = false;
		}

		public void Again()
		{
			uv_timer_again(handle);
		}
	}
}

