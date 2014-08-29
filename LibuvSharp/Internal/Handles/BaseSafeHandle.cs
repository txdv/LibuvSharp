using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public abstract class BaseSafeHandle : SafeHandle
	{
		public BaseSafeHandle()
			: base(IntPtr.Zero, true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return ReleaseHandleImpl();
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		protected abstract bool ReleaseHandleImpl();

		public override bool IsInvalid {
			get {
				return IsInvalidImpl();
			}
		}

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		public virtual bool IsInvalidImpl()
		{
			return handle == IntPtr.Zero;
		}

		public IntPtr Handle {
			get {
				return this.handle;
			}
		}
	}
}

