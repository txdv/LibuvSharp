using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Mutex : BaseHandle
	{
		[DllImport("uv")]
		internal extern static int uv_mutex_init(IntPtr ptr);

		[DllImport("uv")]
		internal extern static void uv_mutex_destroy(IntPtr ptr);

		public Mutex()
			: base(50, (Func<IntPtr, int>)uv_mutex_init, uv_mutex_destroy)
		{
		}

		[DllImport("uv")]
		internal extern static void uv_mutex_lock(IntPtr ptr);

		public void Lock()
		{
			uv_mutex_lock(Handle);
		}

		[DllImport("uv")]
		internal extern static int uv_mutex_trylock(IntPtr ptr);

		public bool TryLock()
		{
			return uv_mutex_trylock(Handle) == 0;
		}

		[DllImport("uv")]
		internal extern static void uv_mutex_unlock(IntPtr ptr);

		public void Unlock()
		{
			uv_mutex_unlock(Handle);
		}
	}
}

