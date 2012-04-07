using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	unsafe public class LoadAverage
	{
		[DllImport("uv")]
		internal static extern void uv_loadavg(IntPtr avg);

		internal LoadAverage()
		{
			IntPtr ptr = Marshal.AllocHGlobal(sizeof(double) * 3);
			uv_loadavg(ptr);
			Last = *((double *)ptr);
			Five = *((double *)(ptr + sizeof(double)));
			Fifteen = *((double *)(ptr + sizeof(double) * 2));
			Marshal.FreeHGlobal(ptr);
		}

		public double Last { get; protected set; }
		public double Five { get; protected set; }
		public double Fifteen { get; protected set; }

	}

	public static class Computer
	{
		public static class Memory
		{
			[DllImport("uv")]
			internal static extern long uv_get_free_memory();

			public static long Free {
				get {
					return uv_get_free_memory();
				}
			}

			[DllImport("uv")]
			internal static extern long uv_get_total_memory();

			public static long Total {
				get {
					return uv_get_total_memory();
				}
			}
		}

		[DllImport("uv")]
		internal static extern ulong uv_hrtime();

		public static ulong HighResolutionTime {
			get {
				return uv_hrtime();
			}
		}

		public static LoadAverage Load {
			get {
				return new LoadAverage();
			}
		}
	}
}

