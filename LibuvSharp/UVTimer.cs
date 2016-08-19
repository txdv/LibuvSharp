using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class UVTimer : Handle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void uv_timer_cb(IntPtr loop);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_timer_init(IntPtr loop, IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_timer_start(IntPtr timer, uv_timer_cb callback, ulong timeout, ulong repeat);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_timer_stop(IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_timer_again(IntPtr timer);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_timer_set_repeat(IntPtr timer, ulong repeat);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern ulong uv_timer_get_repeat(IntPtr timer);

		Action onehit;

		public UVTimer()
			: this(Loop.Constructor)
		{
		}

		public UVTimer(Loop loop)
			: base(loop, HandleType.UV_TIMER, uv_timer_init)
		{
		}

		public ulong LongRepeat {
			get {
				CheckDisposed();

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
				LongRepeat = (ulong)value.TotalMilliseconds;
			}
		}

		public bool Running { get; private set; }

		public void Start(ulong timeout, ulong repeat)
		{
			CheckDisposed();

			if (Running) {
				Stop();
			}
			Running = true;
			LongRepeat = repeat;

			int r = uv_timer_start(NativeHandle, cb, timeout, repeat);
			Ensure.Success(r);
		}

		static uv_timer_cb cb = OnTick;

		static void OnTick(IntPtr handle)
		{
			FromIntPtr<UVTimer>(handle).OnTick();
		}

		void OnTick()
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

		public void Start(ulong repeat)
		{
			Start(0, repeat);
		}
		public void Start(TimeSpan timeout, TimeSpan repeat)
		{
			Start((ulong)timeout.TotalMilliseconds, (ulong)repeat.TotalMilliseconds);
		}
		public void Start(TimeSpan repeat)
		{
			Start(TimeSpan.Zero, repeat);
		}

		public void Start(ulong timeout, Action callback)
		{
			onehit = callback;
			Start(timeout, 0);
		}
		public void Start(TimeSpan timeout, Action callback)
		{
			Start((ulong)timeout.TotalMilliseconds, callback);
		}

		public void Stop()
		{
			CheckDisposed();

			if (Running) {
				int r = uv_timer_stop(NativeHandle);
				Ensure.Success(r);
			}
			Running = false;
		}

		public void Again()
		{
			Invoke(uv_timer_again);
		}

		public static UVTimer Once(Loop loop, TimeSpan timeout, Action callback)
		{
			var timer = new UVTimer(loop);
			timer.Tick += () => {
				if (callback != null) {
					callback();
				}
				timer.Close();
			};
			timer.Start(timeout, TimeSpan.Zero);
			return timer;
		}

		public static UVTimer Once(TimeSpan timeout, Action callback)
		{
			return Once(Loop.Constructor, timeout, callback);
		}

		public static UVTimer Times(Loop loop, int times, TimeSpan repeat, Action<int> callback)
		{
			var timer = new UVTimer(loop);
			int i = 0;
			timer.Tick += () => {
				i++;
				if (callback != null) {
					callback(i);
				}
				if (i >= times) {
					timer.Close();
				}
			};
			timer.Start(repeat, repeat);
			return timer;
		}

		public static UVTimer Times(int times, TimeSpan repeat, Action<int> callback)
		{
			return Times(Loop.Constructor, times, repeat, callback);
		}

		public static UVTimer Every(Loop loop, TimeSpan repeat, Action callback)
		{
			var timer = new UVTimer(loop);
			timer.Tick += callback;
			timer.Start(repeat, repeat);
			return timer;
		}

		public static UVTimer Every(TimeSpan repeat, Action callback)
		{
			return Every(Loop.Constructor, repeat, callback);
		}
	}
}

