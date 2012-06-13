using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe internal class PermaRequest : IDisposable
	{
		public IntPtr Handle { get; protected set; }

		protected uv_req_t *request;

		public IntPtr Data {
			get {
				return request->data;
			}
			set {
				request->data = value;
			}
		}

		public GCHandle GCHandle {
			get {
				GCHandle *p = (GCHandle *)Data;
				return *p;
			}
			set {
				GCHandle *p = (GCHandle *)Data;
				*p = value;
			}
		}

		public PermaRequest(int size)
			: this(size, true)
		{
		}

		public PermaRequest(int size, bool allocate)
			: this(UV.Alloc(size), allocate)
		{
		}

		public PermaRequest(IntPtr ptr)
			: this(ptr, true)
		{
		}

		protected PermaRequest(IntPtr handle, bool allocate)
		{
			Handle = handle;
			request = (uv_req_t *)handle;

			Data = IntPtr.Zero;

			if (allocate) {
				Data = UV.Alloc(sizeof(GCHandle));
			}

			GCHandle = GCHandle.Alloc(this, GCHandleType.Normal);
		}

		public virtual void Dispose()
		{
			Dispose(true);
		}

		public virtual void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}

			if (Data != IntPtr.Zero) {
				if (GCHandle.IsAllocated) {
					GCHandle.Free();
				}
				UV.Free(Data);
				Data = IntPtr.Zero;
			}

			if (Handle != IntPtr.Zero) {
				UV.Free(Handle);
				Handle = IntPtr.Zero;
			}
		}

		unsafe public static T GetObject<T>(IntPtr ptr) where T : class
		{
			uv_req_t *req = (uv_req_t *)ptr.ToPointer();
			GCHandle *gchandle = (GCHandle *)req->data;
			return (gchandle->Target as T);
		}
	}
}
