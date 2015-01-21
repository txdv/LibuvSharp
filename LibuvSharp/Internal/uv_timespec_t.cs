using System;

namespace LibuvSharp
{
	struct uv_timespec_t {
		public long tv_sec;
		public long tv_nsec;

		public DateTime ToDateTime()
		{
			return new DateTime(1970, 1, 1, 0, 0, 0)
				+ TimeSpan.FromSeconds(tv_sec)
				+ TimeSpan.FromTicks(tv_nsec / 100);
		}
	}
}

