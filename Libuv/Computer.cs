using System;
using System.Runtime.InteropServices;

namespace Libuv
{
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
	}
}

