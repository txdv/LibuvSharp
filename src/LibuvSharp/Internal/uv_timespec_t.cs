using System;

namespace LibuvSharp
{
	struct uv_timespec_t {
		public IntPtr tv_sec;
		public IntPtr tv_nsec;

		public DateTime ToDateTime()
		{
			return new DateTime(1970, 1, 1, 0, 0, 0)
				+ TimeSpan.FromSeconds(tv_sec.ToInt64())
				+ TimeSpan.FromTicks(tv_nsec.ToInt64() / 100);
		}
	}
}

