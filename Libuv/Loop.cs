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

			if (Handle != Default.Handle) {
				uv_loop_delete(Handle);
			}
		}
	}
}

