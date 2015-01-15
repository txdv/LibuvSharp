using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe public abstract class Handle : IHandle, IDisposable
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate void callback(IntPtr req, int status);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate UnixBufferStruct alloc_callback_unix(IntPtr data, int size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate WindowsBufferStruct alloc_callback_win(IntPtr data, int size);

		public Loop Loop { get; protected set; }

		public IntPtr NativeHandle { get; protected set; }

		internal GCHandle GCHandle { get; set; }

		uv_handle_t *handle {
			get {
				return (uv_handle_t *)NativeHandle;
			}
		}

		public IntPtr DataPointer {
			get {
				return handle->data;
			}
			set {
				handle->data = value;
			}
		}

		public HandleType HandleType {
			get {
				return handle->type;
			}
		}

		internal Handle(Loop loop, IntPtr handle)
		{
			Ensure.ArgumentNotNull(loop, "loop");

			NativeHandle = handle;
			GCHandle = GCHandle.Alloc(this);
			Loop = loop;

			Loop.handles[NativeHandle] = this;
		}

		internal Handle(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal Handle(Loop loop, HandleType type)
			: this(loop, Handle.Size(type))
		{
		}

		public event Action Closed;

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_close(IntPtr handle, Action callback);

		public void Close(Action callback)
		{
			if (NativeHandle != IntPtr.Zero) {

				var nativeHandle = NativeHandle;

				CAction ca = new CAction(() => {
					// Remove handle
					Loop.handles.Remove(nativeHandle);

					UV.Free(nativeHandle);

					if (Closed != null) {
						Closed();
					}

					if (callback != null) {
						callback();
					}

					if (GCHandle.IsAllocated) {
						GCHandle.Free();
					}
				});

				uv_close(NativeHandle, ca.Callback);
				NativeHandle = IntPtr.Zero;
			}
		}

		public void Close()
		{
			Close(null);
		}

		public bool IsClosed {
			get {
				return NativeHandle == IntPtr.Zero;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			Close();
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_active(IntPtr handle);

		public bool Active {
			get {
				return uv_is_active(NativeHandle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_is_closing(IntPtr handle);

		public bool Closing {
			get {
				return uv_is_closing(NativeHandle) != 0;
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_ref(IntPtr handle);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern void uv_unref(IntPtr handle);

		public void Ref()
		{
			uv_ref(NativeHandle);
		}

		public void Unref()
		{
			uv_unref(NativeHandle);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_handle_size(HandleType type);

		public static int Size(HandleType type)
		{
			return uv_handle_size(type);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern HandleType uv_guess_handle(int fd);

		public static HandleType Guess(int fd)
		{
			return uv_guess_handle(fd);
		}
	}
}

