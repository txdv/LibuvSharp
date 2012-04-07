using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Libuv
{
	unsafe internal struct uv_interface_address_t
	{
		public IntPtr name;
		public int is_internal;
		public sockaddr_in6 sockaddr;
	}

	unsafe public class NetworkInterface
	{
		internal NetworkInterface(uv_interface_address_t *iface)
		{
			Name = Marshal.PtrToStringAnsi(iface->name);
			Internal = iface->is_internal != 0;
			Address = UV.GetIPEndPoint(new IntPtr(iface) + sizeof(IntPtr) + sizeof(int)).Address;
		}

		public string Name { get; protected set; }
		public bool Internal { get; protected set; }
		public IPAddress Address { get; protected set; }


		[DllImport("uv")]
		internal static extern uv_err_t uv_interface_addresses(out IntPtr address, out int count);

		[DllImport("uv")]
		internal static extern void uv_free_interface_addresses(IntPtr address, int count);

		internal static NetworkInterface[] GetInterfaces()
		{
			IntPtr interfaces;
			int count;
			var error = uv_interface_addresses(out interfaces, out count);

			NetworkInterface[] ret = new NetworkInterface[count];

			for (int i = 0; i < count; i++) {
				uv_interface_address_t *iface = (uv_interface_address_t *)(interfaces + i*sizeof(uv_interface_address_t));
				ret[i] = new NetworkInterface(iface);
			}

			uv_free_interface_addresses(interfaces, count);
			UV.EnsureSuccess(error);
			return ret;
		}
	}

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

			public static long Used {
				get {
					return Total - Free;
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

		public static NetworkInterface[] NetworkInterfaces {
			get {
				return NetworkInterface.GetInterfaces();
			}
		}
	}
}

