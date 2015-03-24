using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct sockaddr
	{
		public short sin_family;
		public ushort sin_port;
	}

	[StructLayout(LayoutKind.Sequential, Size=16)]
	internal struct sockaddr_in
	{
		public int a, b, c, d;
	}

	[StructLayout(LayoutKind.Sequential, Size=28)]
	internal struct sockaddr_in6
	{
		public int a, b, c, d, e, f, g;
	}

	public static class UV
	{
		unsafe internal static readonly int PointerSize = sizeof(IntPtr) / 4;

		internal static bool isUnix = (System.Environment.OSVersion.Platform == PlatformID.Unix) || (System.Environment.OSVersion.Platform == PlatformID.MacOSX);
		internal static bool IsUnix { get { return isUnix; } }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip4_addr(string ip, int port, out sockaddr_in address);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip6_addr(string ip, int port, out sockaddr_in6 address);

		internal static sockaddr_in ToStruct(string ip, int port)
		{
			sockaddr_in address;
			int r = uv_ip4_addr(ip, port, out address);
			Ensure.Success(r);
			return address;
		}

		internal static sockaddr_in6 ToStruct6(string ip, int port)
		{
			sockaddr_in6 address;
			int r = uv_ip6_addr(ip, port, out address);
			Ensure.Success(r);
			return address;
		}

		[DllImport("__Internal", EntryPoint = "ntohs", CallingConvention = CallingConvention.Cdecl)]
		internal extern static ushort ntohs_unix(ushort bytes);

		[DllImport("Ws2_32", EntryPoint = "ntohs")]
		internal extern static ushort ntohs_win(ushort bytes);

		internal static ushort ntohs(ushort bytes)
		{
			if (isUnix) {
				return ntohs_unix(bytes);
			} else {
				return ntohs_win(bytes);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip4_name(IntPtr src, byte[] dst, IntPtr size);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static int uv_ip6_name(IntPtr src, byte[] dst, IntPtr size);

		unsafe internal static IPEndPoint GetIPEndPoint(IntPtr sockaddr)
		{
			sockaddr *sa = (sockaddr *)sockaddr;
			byte[] addr = new byte[64];
			int r;
			if (sa->sin_family == 2) {
				r = uv_ip4_name(sockaddr, addr, (IntPtr)addr.Length);
			} else {
				r = uv_ip6_name(sockaddr, addr, (IntPtr)addr.Length);
			}
			Ensure.Success(r);

			int i = 0;
			while (i < addr.Length && addr[i] != 0) {
				i++;
			}

			IPAddress ip = IPAddress.Parse(System.Text.Encoding.ASCII.GetString(addr, 0, i));

			return new IPEndPoint(ip, ntohs(sa->sin_port));
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern int uv_req_size(RequestType type);

		internal static int Sizeof(RequestType type)
		{
			return uv_req_size(type);
		}

#if DEBUG
		static HashSet<IntPtr> pointers = new HashSet<IntPtr>();
#endif

		internal static IntPtr Alloc(RequestType type)
		{
			return Alloc(Sizeof(type));
		}

		internal static IntPtr Alloc(HandleType type)
		{
			return Alloc(Handle.Size(type));
		}

		internal static IntPtr Alloc(int size)
		{
			IntPtr ptr = Marshal.AllocHGlobal(size);
#if DEBUG
			pointers.Add(ptr);
#endif
			return ptr;
		}

		internal static void Free(IntPtr ptr)
		{
#if DEBUG
			if (pointers.Contains(ptr)) {
				pointers.Remove(ptr);
				Marshal.FreeHGlobal(ptr);
			} else {
				Console.WriteLine("{0} not allocated", ptr);
			}
#else
			Marshal.FreeHGlobal(ptr);
#endif
		}

		unsafe internal static UnixBufferStruct Alloc(IntPtr handle, int size)
		{
			UnixBufferStruct buf;
			buf.@base = Alloc(size);
			buf.length = (IntPtr)size;
			return buf;
		}

		internal static void Free(UnixBufferStruct buf)
		{
			Free(buf.@base);
		}
#if DEBUG
		public static int PointerCount {
			get {
				return pointers.Count;
			}
		}

		unsafe internal static UnixBufferStruct DebugAlloc(IntPtr handle, int size)
		{
			UnixBufferStruct tmp = UV.Alloc(handle, size);
			Console.WriteLine (tmp.@base);
			return tmp;
		}

		public static void PrintPointers()
		{
			var e = pointers.GetEnumerator();
			Console.Write("[");
			if (e.MoveNext()) {
				Console.Write(e.Current);
				while (e.MoveNext()) {
					Console.Write(", ");
					Console.Write(e.Current);
				}
			}
			Console.WriteLine("]");
		}
#endif

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static uint uv_version();

		public static void GetVersion(out int major, out int minor, out int patch)
		{
			uint version = uv_version();
			major = (int)(version & 0xFF0000) >> 16;
			minor = (int)(version & 0xFF00) >> 8;
			patch = (int)(version & 0xFF);
		}

		public static Version Version {
			get {
				int major, minor, patch;
				GetVersion(out major, out minor, out patch);
				return new Version(major, minor, patch);
			}
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		unsafe internal extern static sbyte *uv_version_string();

		unsafe public static string VersionString {
			get {
				return new string(uv_version_string());
			}
		}

		public static bool IsPreRelease {
			get {
				return VersionString.EndsWith("-pre");
			}
		}

		unsafe internal static IPEndPoint GetSockname(Handle handle)
		{
			sockaddr_in6 addr;
			IntPtr ptr = new IntPtr(&addr);
			int length = sizeof(sockaddr_in6);
			int r = NativeMethods.uv_tcp_getsockname(handle.NativeHandle, ptr, ref length);
			Ensure.Success(r);
			return UV.GetIPEndPoint(ptr);
		}

		internal delegate int callback(IntPtr handle, ref IntPtr size);

		internal static string ToString(int size, callback func)
		{
			IntPtr ptr = IntPtr.Zero;
			try {
				ptr = Marshal.AllocHGlobal(size);
				IntPtr sizePointer = (IntPtr)size;
				int r = func(ptr, ref sizePointer);
				Ensure.Success(r);
				return Marshal.PtrToStringAuto(ptr, sizePointer.ToInt32());
			} finally {
				if (ptr != IntPtr.Zero) {
					Marshal.FreeHGlobal(ptr);
				}
			}
		}

		internal static string ToString(int size, Func<IntPtr, IntPtr, int> func)
		{
			IntPtr ptr = IntPtr.Zero;
			try {
				ptr = Marshal.AllocHGlobal(size);
				int r = func(ptr, (IntPtr)size);
				Ensure.Success(r);
				return Marshal.PtrToStringAuto(ptr);
			} finally {
				if (ptr != IntPtr.Zero) {
					Marshal.FreeHGlobal(ptr);
				}
			}
		}
	}
}

