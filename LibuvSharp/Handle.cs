using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class Handle : IDisposable
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void callback(IntPtr req, int status);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate UnixBufferStruct alloc_callback_unix(IntPtr data, int size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate WindowsBufferStruct alloc_callback_win(IntPtr data, int size);

		public Loop Loop { get; protected set; }

		GCHandle GCHandle { get; set; }
		internal IntPtr handle;

		internal Handle(Loop loop, IntPtr handle)
		{
			Ensure.ArgumentNotNull(loop, "loop");

			this.handle = handle;
			GCHandle = GCHandle.Alloc(this);
			Loop = loop;
		}

		internal Handle(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal Handle(Loop loop, UvHandleType type)
			: this(loop, UV.Sizeof(type))
		{
		}

		public event Action CloseEvent;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_close(IntPtr handle, Action callback);

		public void Close(Action callback)
		{
			if (handle == IntPtr.Zero) {
				return;
			}

			CAction ca = new CAction(() => {
				if (CloseEvent != null) {
					CloseEvent();
				}

				if (callback != null) {
					callback();
				}

				Dispose();
			});

			int r = uv_close(handle, ca.Callback);
			Ensure.Success(r, Loop);
		}

		public void Close()
		{
			Close(null);
		}

		public bool Closed {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			UV.Free(handle);
			GCHandle.Free();
			handle = IntPtr.Zero;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_active(IntPtr handle);

		public bool Active {
			get {
				return uv_is_active(handle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_readable(IntPtr handle);

		public bool Readable {
			get {
				return uv_is_readable(handle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_writable(IntPtr handle);

		public bool Writeable {
			get {
				return uv_is_writable(handle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_closing(IntPtr handle);

		public bool Closing {
			get {
				return uv_is_closing(handle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_ref(IntPtr handle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_unref(IntPtr handle);

		public void Ref()
		{
			uv_ref(handle);
		}

		public void Unref()
		{
			uv_unref(handle);
		}

	}
}

