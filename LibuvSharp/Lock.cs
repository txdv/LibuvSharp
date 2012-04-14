using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Lock : BaseHandle
	{
		[DllImport("uv")]
		internal extern static int uv_rwlock_init(IntPtr ptr);

		[DllImport("uv")]
		internal extern static void uv_rwlock_destroy(IntPtr ptr);

		public Lock()
			: base(50, (Func<IntPtr, int>)uv_rwlock_init, uv_rwlock_destroy)
		{
		}

		[DllImport("uv")]
		internal extern static void uv_rwlock_rdlock(IntPtr ptr);

		public void ReadLock()
		{
			uv_rwlock_rdlock(Handle);
		}

		[DllImport("uv")]
		internal extern static int uv_rwlock_tryrdlock(IntPtr ptr);

		public bool TryReadLock()
		{
			return uv_rwlock_tryrdlock(Handle) != 0;
		}

		[DllImport("uv")]
		internal extern static void uv_rwlock_rdunlock(IntPtr ptr);

		public void ReadUnlock()
		{
			uv_rwlock_rdunlock(Handle);
		}

		[DllImport("uv")]
		internal extern static void uv_rwlock_wrlock(IntPtr ptr);

		public void WriteLock()
		{
			uv_rwlock_wrlock(Handle);
		}

		[DllImport("uv")]
		internal extern static int uv_rwlock_trywrlock(IntPtr ptr);

		public bool TryWriteLock()
		{
			return uv_rwlock_trywrlock(Handle) != 0;
		}

		[DllImport("uv")]
		internal extern static void uv_rwlock_wrunlock(IntPtr ptr);

		public void WriteUnlock()
		{
			uv_rwlock_wrunlock(Handle);
		}
	}
}

