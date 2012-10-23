using System;
using System.Net;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe internal struct uv_cpu_times_t
	{
		public ulong user;
		public ulong nice;
		public ulong sys;
		public ulong idle;
		public ulong irq;
	}

	unsafe internal struct uv_cpu_info_t
	{
		public IntPtr model;
		public int speed;
		public uv_cpu_times_t cpu_times;
	}

	public class CpuTimes
	{
		internal CpuTimes(uv_cpu_times_t times)
		{
			User = times.user;
			Nice = times.nice;
			System = times.sys;
			Idle = times.idle;
			IRQ = times.irq;
		}

		public ulong User { get; protected set; }
		public ulong Nice { get; protected set; }
		public ulong System { get; protected set; }
		public ulong Idle { get; protected set; }
		public ulong IRQ { get; protected set; }
	}

	unsafe public class CpuInformation
	{
		internal CpuInformation(uv_cpu_info_t *info)
		{
			Name = Marshal.PtrToStringAnsi(info->model);
			Speed = info->speed;
			Times = new CpuTimes(info->cpu_times);
		}

		public string Name { get; protected set; }
		public int Speed { get; protected set; }
		public CpuTimes Times { get; protected set; }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_cpu_info(out IntPtr info, out int count);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_free_cpu_info(IntPtr info, int count);

		internal static CpuInformation[] GetInfo()
		{
			IntPtr info;
			int count;
			var error = uv_cpu_info(out info, out count);

			CpuInformation[] ret = new CpuInformation[count];

			for (int i = 0; i < count; i++) {
				uv_cpu_info_t *cpuinfo = (uv_cpu_info_t *)(info.ToInt64() + i * sizeof(uv_cpu_info_t));
				ret[i] = new CpuInformation(cpuinfo);
			}

			uv_free_cpu_info(info, count);
			Ensure.Success(error);
			return ret;
		}
	}

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
			Address = UV.GetIPEndPoint((IntPtr)(((IntPtr)iface).ToInt64() + sizeof(IntPtr) + sizeof(int))).Address;
		}

		public string Name { get; protected set; }
		public bool Internal { get; protected set; }
		public IPAddress Address { get; protected set; }


		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_interface_addresses(out IntPtr address, out int count);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_free_interface_addresses(IntPtr address, int count);

		internal static NetworkInterface[] GetInterfaces()
		{
			IntPtr interfaces;
			int count;
			var error = uv_interface_addresses(out interfaces, out count);

			NetworkInterface[] ret = new NetworkInterface[count];

			for (int i = 0; i < count; i++) {
				uv_interface_address_t *iface = (uv_interface_address_t *)(interfaces.ToInt64() + i * sizeof(uv_interface_address_t));
				ret[i] = new NetworkInterface(iface);
			}

			uv_free_interface_addresses(interfaces, count);
			Ensure.Success(error);
			return ret;
		}
	}

	unsafe public class LoadAverage
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern void uv_loadavg(IntPtr avg);

		internal LoadAverage()
		{
			IntPtr ptr = Marshal.AllocHGlobal(sizeof(double) * 3);
			uv_loadavg(ptr);
			Last = *((double *)ptr);
			Five = *((double *)(ptr.ToInt64() + sizeof(double)));
			Fifteen = *((double *)(ptr.ToInt64() + sizeof(double) * 2));
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
			[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
			internal static extern long uv_get_free_memory();

			public static long Free {
				get {
					return uv_get_free_memory();
				}
			}

			[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
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

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
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

		public static CpuInformation[] CpuInfo {
			get {
				return CpuInformation.GetInfo();
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_uptime(out double uptime);

		public static double Uptime {
			get {
				double uptime;
				Ensure.Success(uv_uptime(out uptime));
				return uptime;
			}
		}
	}
}

