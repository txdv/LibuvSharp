using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class UVTimer : Handle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_init(IntPtr loop, IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_start(IntPtr timer, callback callback, long timeout, long repeat);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_stop(IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_timer_again(IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_timer_set_repeat(IntPtr timer, long repeat);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern long uv_timer_get_repeat(IntPtr timer);

		callback cb;
		Action onehit;

		public UVTimer()
			: this(Loop.Default)
		{
		}

		public UVTimer(Loop loop)
			: base(loop, HandleType.UV_TIMER)
		{
			uv_timer_init(loop.NativeHandle, NativeHandle);
			cb = OnTick;
		}

		public long LongRepeat {
			get {
				return uv_timer_get_repeat(NativeHandle);
			}
			set {
				uv_timer_set_repeat(NativeHandle, value);
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

		public void Start(long timeout, long repeat)
		{
			if (Running) {
				Stop();
			}
			Running = true;
			LongRepeat = repeat;
			uv_timer_start(NativeHandle, cb, timeout, repeat);
		}

		void OnTick(IntPtr handle, int status)
		{
			var cb = onehit;
			if (cb != null) {
				// ensure onehit is null when invoking
				onehit = null;
				cb();
			}

			if (Tick != null) {
				Tick();
			}
		}

		public event Action Tick;

		public void Start(long repeat)
		{
			Start(0, repeat);
		}
		public void Start(TimeSpan timeout, TimeSpan repeat)
		{
			Start((long)timeout.TotalMilliseconds, (long)repeat.TotalMilliseconds);
		}
		public void Start(TimeSpan repeat)
		{
			Start(TimeSpan.Zero, repeat);
		}

		public void Start(long timeout, Action callback)
		{
			onehit = callback;
			Start(timeout, 0);
		}
		public void Start(TimeSpan timeout, Action callback)
		{
			Start((long)timeout.TotalMilliseconds, callback);
		}

		public void Stop()
		{
			if (Running) {
				uv_timer_stop(NativeHandle);
			}
			Running = false;
		}

		public void Again()
		{
			uv_timer_again(NativeHandle);
		}
	}
}

