using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Loop : IDisposable
	{
		[DllImport("uv")]
		internal static extern IntPtr uv_default_loop();

		[DllImport("uv")]
		internal static extern IntPtr uv_loop_new();

		[DllImport("uv")]
		internal static extern void uv_loop_delete(IntPtr ptr);

		[DllImport("uv")]
		internal static extern void uv_run(IntPtr loop);

		[DllImport("uv")]
		internal static extern void uv_run_once(IntPtr loop);

		[DllImport("uv")]
		internal static extern void uv_ref(IntPtr loop);

		[DllImport("uv")]
		internal static extern void uv_unref(IntPtr loop);

		[DllImport("uv")]
		internal static extern void uv_update_time(IntPtr loop);

		[DllImport("uv")]
		internal static extern long uv_now(IntPtr loop);

		internal static Loop @default = new Loop(uv_default_loop());
		public static Loop Default {
			get {
				return @default;
			}
		}

		internal IntPtr ptr;

		internal Loop(IntPtr ptr)
		{
			this.ptr = ptr;
		}

		public Loop()
			: this(uv_loop_new())
		{
		}

		public void Run()
		{
			uv_run(ptr);
		}

		public void RunOnce()
		{
			uv_run_once(ptr);
		}

		public void Ref()
		{
			uv_ref(ptr);
		}

		public void Unref()
		{
			uv_unref(ptr);
		}

		public void UpdateTime()
		{
			uv_update_time(ptr);
		}

		public long Now {
			get {
				return uv_now(ptr);
			}
		}

		#region IDisposable implementation
		public void Dispose()
		{
			//uv_loop_delete(ptr);
		}
		#endregion
	}
}

