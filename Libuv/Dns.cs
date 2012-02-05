using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class Hostent
	{
		public Hostent(string hostname, IPAddress[] addresses, string[] aliases)
		{
			Hostname = hostname;
			IPAddresses = addresses;
			Aliases = aliases;
		}

		public string Hostname { get; protected set; }
		public IPAddress[] IPAddresses { get; protected set; }
		public string[] Aliases { get; protected set; }
	}

	public class Dns
	{
		enum LibraryInit
		{
			None = 0,
			Win32 = 1,
			All = Win32
		}

		public enum Result : int
		{
			SUCESS = 0,

			ENODATA,
			EFORMER,
			ESERVFAIL,
			ENOTFOUND,
			ENOTIMP,
			EREFESUED,

			EBADQUERY,
			EBADNAME,
			EBADFAMILY,
			EBADRESP,
			ECONNREFUSED,
			ETIMEOUT,
			EOF,
			EFILE,
			ENOMEM,
			EDESTRUCTION,
			EBADSTR,

			EBADFLAGS,
			ENONAME,
			EBADHINTS,
			ENOTINITIALIZED,
			ELOADIPHLPAPI,
			EADDRGETNETWORKPARAMS
		}

		unsafe struct hostent {
			public sbyte *name;
			public sbyte **aliases;
			public int addrtype;
			public int length;
			public sbyte **addrlist;

			public AddressFamily AddressFamily {
				get {
					return GetAddressFamily(addrtype);
				}
			}

			static AddressFamily GetAddressFamily(int addrtype)
			{
				switch (addrtype) {
				case 1:
					return AddressFamily.InterNetwork;
				case 28:
					return AddressFamily.InterNetworkV6;
				default:
					return AddressFamily.Unknown;
				}
			}

			static void Each(sbyte **iterator, Action<IntPtr> callback)
			{
				int i = 0;
				for (sbyte *j = iterator[0]; j != (sbyte *)0; j = iterator[++i]) {
					callback(new IntPtr(j));
				}
			}

			IPAddress[] ToAddress()
			{
				List<IPAddress> list = new List<IPAddress>();
				int size = 128;
				IntPtr dst = Marshal.AllocHGlobal(size);

				var that = this;

				hostent.Each(addrlist, (src) => {
					inet_ntop(that.addrtype, src, dst, new IntPtr(size));
					string ip = new string((sbyte *)dst.ToPointer());
					list.Add(IPAddress.Parse(ip));
				});

				Marshal.FreeHGlobal(dst);
				return list.ToArray();
			}

			string[] ToAliases()
			{
				List<string> l = new List<string>();
				hostent.Each(aliases, (src) => {
					l.Add(new string((sbyte *)src));
				});
				return l.ToArray();
			}

			public Hostent ToHostent()
			{
				return new Hostent(Hostname, ToAddress(), ToAliases());
			}

			public static Hostent GetHostent(IntPtr ptr)
			{
				return ((hostent *)ptr)->ToHostent();
			}

			public string Hostname {
				get {
					return new string(name);
				}
			}

			[DllImport("__Internal")]
			unsafe static extern sbyte *inet_ntop(int af, IntPtr src, IntPtr dst, IntPtr size);
		}

		unsafe struct ares_options {
			public int flags;
			public int timeout;
			public int tries;
			public int ndots;
			public ushort udp_port;
			public ushort tcp_port;
			public int socket_send_buffer_size;
			public int socket_receive_buffer_size;
			public IntPtr servers;
			public int nservers;
			public sbyte **domains;
			public int ndomains;
			public sbyte *lookups;
			public IntPtr sock_state_cb;
			public IntPtr sock_state_cb_data;
			public IntPtr sortlist;
			public int nsort;
		}

		[DllImport("uv")]
		static extern int ares_library_init(LibraryInit flags);

		[DllImport("uv")]
		static extern int uv_ares_init_options(IntPtr loop, ref IntPtr channel, IntPtr options, int optmask);

		[DllImport("uv")]
		static extern void uv_ares_destroy(IntPtr loop, IntPtr channel);

		[DllImport("uv")]
		static extern int ares_query(IntPtr channel, string name, int dnsclass, int type, Action<IntPtr, int, int, IntPtr, int> callback, IntPtr arg);

		[DllImport("uv")]
		static extern int ares_parse_a_reply(IntPtr buf, int alen, ref IntPtr host, IntPtr addrttls, IntPtr naddrttls);

		[DllImport("uv")]
		static extern int ares_parse_aaaa_reply(IntPtr buf, int alen, ref IntPtr host, IntPtr addrttls, IntPtr naddrttls);

		[DllImport("uv")]
		static extern void ares_free_hostent(IntPtr host);

		public Loop Loop { get; protected set; }

		IntPtr channel;
		IntPtr options;

		static Dns()
		{
			ares_library_init(LibraryInit.All);
		}

		public Dns(Loop loop)
		{
			Loop = loop;
			options = Marshal.AllocHGlobal(UV.Sizeof(UvType.AresOptions));
			int r = uv_ares_init_options(loop.Handle, ref channel, options, 0);
			UV.EnsureSuccess(r);
		}

		~Dns()
		{
			uv_ares_destroy(Loop.Handle, channel);
			Marshal.FreeHGlobal(options);
		}

		unsafe class Callback : IDisposable
		{
			public IntPtr Handle { get; protected set; }

			public GCHandle GCHandle {
				get {
					return *((GCHandle *)Handle.ToPointer());
				}
				set {
					*((GCHandle *)Handle.ToPointer()) = value;
				}
			}

			public Callback()
			{
				Handle = Marshal.AllocHGlobal(sizeof(GCHandle));
				GCHandle = GCHandle.Alloc(this, GCHandleType.Normal);
			}

			public void Dispose()
			{
				if (Handle != IntPtr.Zero) {
					GCHandle.Free();
					Handle = IntPtr.Zero;
				}
			}

			public static T GetObject<T>(IntPtr arg) where T : class
			{
				var handle = *((GCHandle *)arg.ToPointer());
				return handle.Target as T;
			}
		}

		class AresCallback : Callback
		{
			Action<Exception, Hostent> cb;

			public AresCallback(Action<Exception, Hostent> callback)
			{
				cb = callback;
			}

			void End(Exception exception, Hostent hostent)
			{
				if (cb != null) {
					cb(exception, hostent);
				}
				Dispose();
			}

			delegate int AresParseDelegate(IntPtr buf, int alen, ref IntPtr host, IntPtr addrttls, IntPtr naddrttls);

			void Parse(AresParseDelegate ares_parse, IntPtr buf, int alen)
			{
				IntPtr host;
				int r = ares_parse(buf, alen, ref host, IntPtr.Zero, IntPtr.Zero);
				if (r != 0) {
					End(new Exception(string.Format("the parse method returned {0}", r)), null);
					return;
				}
				var he = hostent.GetHostent(host);
				ares_free_hostent(host);
				End(null, he);
			}

			public static void Callback(IntPtr arg, int status, int timeouts, IntPtr buf, int alen)
			{
				var cb = GetObject<AresCallback>(arg);
				cb.Parse(ares_parse_a_reply, buf, alen);
			}

			public static void Callback6(IntPtr arg, int status, int timeouts, IntPtr buf, int alen)
			{
				var cb = GetObject<AresCallback>(arg);
				cb.Parse(ares_parse_aaaa_reply, buf, alen);
			}
		}

		public void Resolve(string host, AddressFamily addressFamily, Action<Exception, Hostent> callback)
		{
			switch (addressFamily) {
			case AddressFamily.InterNetwork:
			case AddressFamily.InterNetworkV6:
				var cb = new AresCallback((exception, address) => {
					callback(exception, address);
				});
				if (addressFamily == AddressFamily.InterNetwork) {
					ares_query(channel, host, 1, 1, AresCallback.Callback, cb.Handle);
				} else {
					ares_query(channel, host, 1, 28, AresCallback.Callback6, cb.Handle);
				}
				break;
			default:
				callback(new ArgumentException("addressFamily, protocol not supported"), null);
				break;
			}
		}

		public void Resolve(string host, Action<Exception, Hostent> callback)
		{
			Resolve(host, AddressFamily.InterNetwork, callback);
		}

		public void Resolve6(string host, Action<Exception, Hostent> callback)
		{
			Resolve(host, AddressFamily.InterNetworkV6, callback);
		}
	}
}

