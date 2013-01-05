using System;

namespace LibuvSharp
{
	public class BaseHandle : IDisposable
	{
		internal IntPtr Handle { get; set; }
		private Action<IntPtr> destroy;

		public BaseHandle(int size)
		{
			Handle = UV.Alloc(size);
		}

		public BaseHandle(int size, Action<IntPtr> init, Action<IntPtr> destroy)
			: this(size, (ptr) => {
				if (init != null) {
					init(ptr);
				}
				return 0;
			}, destroy)
		{
		}

		public BaseHandle(int size, Func<IntPtr, int> init, Action<IntPtr> destroy)
		{
			Handle = UV.Alloc(size);
			if (init != null) {
				int r = init(Handle);
				Ensure.Success(r);
			}

			this.destroy = destroy;
		}

		~BaseHandle()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				if (destroy != null) {
					destroy(Handle);
				}

				UV.Free(Handle);
				Handle = IntPtr.Zero;
			}
		}
	}
}

