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

	public class MailExchange
	{
		public MailExchange(string host, int priority)
		{
			Host = host;
			Priority = priority;
		}

		public string Host { get; protected set; }
		public int Priority { get; protected set; }
	}

	public class Dns : IDisposable
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
				IntPtr dst = UV.Alloc(size);

				var that = this;

				hostent.Each(addrlist, (src) => {
					inet_ntop(that.addrtype, src, dst, new IntPtr(size));
					string ip = new string((sbyte *)dst.ToPointer());
					list.Add(IPAddress.Parse(ip));
				});

				UV.Free(dst);
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

		unsafe struct ares_mx_reply {
			public ares_mx_reply *next;
			public sbyte *host;
			public ushort priority;

			public static MailExchange[] ToMailExchange(IntPtr reply)
			{
				List<MailExchange> list = new List<MailExchange>();
				ares_mx_reply *mx_reply = (ares_mx_reply *)reply;
				while (mx_reply != (ares_mx_reply *)0) {
					list.Add(new MailExchange(new string(mx_reply->host), mx_reply->priority));

					mx_reply = mx_reply->next;
				}
				return list.ToArray();
			}
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
		static extern int ares_parse_a_reply(IntPtr buf, int alen, out IntPtr host, IntPtr addrttls, IntPtr naddrttls);

		[DllImport("uv")]
		static extern int ares_parse_aaaa_reply(IntPtr buf, int alen, out IntPtr host, IntPtr addrttls, IntPtr naddrttls);

		[DllImport("uv")]
		static extern void ares_free_hostent(IntPtr host);

		[DllImport("uv")]
		unsafe static extern int ares_parse_mx_reply(IntPtr abuf, int alen, out IntPtr mx_out);

		[DllImport("uv")]
		static extern void ares_free_data(IntPtr data);

		[DllImport("uv")]
		unsafe static extern sbyte *ares_version(IntPtr ptr);

		public Loop Loop { get; protected set; }

		IntPtr channel;
		IntPtr options;

		public static string Version { get; private set; }

		unsafe static Dns()
		{
			ares_library_init(LibraryInit.All);
			Version = new string(ares_version(IntPtr.Zero));
		}

		internal Dns(Loop loop)
		{
			Loop = loop;
			options = UV.Alloc(1000);
			channel = IntPtr.Zero;

			int r = uv_ares_init_options(loop.Handle, ref channel, options, 0);
			UV.EnsureSuccess(r, loop);
		}

		~Dns()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		void Dispose(bool disposing)
		{
			if (disposing) {
				GC.SuppressFinalize(this);
			}
			if (Loop.Handle != IntPtr.Zero) {
				uv_ares_destroy(Loop.Handle, channel);
				Loop.Handle = IntPtr.Zero;
			}

			if (options != IntPtr.Zero) {
				UV.Free(options);
				options = IntPtr.Zero;
			}
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
				Handle = UV.Alloc(sizeof(GCHandle));
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
				if (arg == IntPtr.Zero) {
					return default(T);
				}

				var handle = *((GCHandle *)arg.ToPointer());
				return handle.Target as T;
			}
		}

		class AresCallback<T> : Callback where T : class
		{
			Action<Exception, T> cb;

			public AresCallback(Action<Exception, T> callback)
			{
				cb = callback;
			}

			public void End(Exception exception, T arg1)
			{
				if (cb != null) {
					cb(exception, arg1);
				}
				Dispose();
			}
		}

		delegate int AresParseDelegate(IntPtr buf, int alen, out IntPtr host, IntPtr addrttls, IntPtr naddrttls);

		static void Parse(AresCallback<Hostent> cb, AresParseDelegate ares_parse, IntPtr buf, int alen)
		{
			IntPtr host;
			int r = ares_parse(buf, alen, out host, IntPtr.Zero, IntPtr.Zero);
			if (r != 0) {
				cb.End(new Exception(string.Format("the parse method returned {0}", r)), null);
				return;
			}
			var he = hostent.GetHostent(host);
			cb.End(null, he);
		}

		static void Callback4(IntPtr arg, int status, int timeouts, IntPtr buf, int alen)
		{
			var cb = Callback.GetObject<AresCallback<Hostent>>(arg);
			Parse(cb, ares_parse_a_reply, buf, alen);
		}

		static void Callback6(IntPtr arg, int status, int timeouts, IntPtr buf, int alen)
		{
			var cb = Callback.GetObject<AresCallback<Hostent>>(arg);
			Parse(cb, ares_parse_aaaa_reply, buf, alen);
		}

		static void CallbackMx(IntPtr arg, int status, int timeouts, IntPtr buf, int alen)
		{
			var cb = Callback.GetObject<AresCallback<MailExchange[]>>(arg);
			IntPtr reply;
			int r = ares_parse_mx_reply(buf, alen, out reply);
			if (r != 0) {
				cb.End(new Exception(string.Format("the parse method returned {0}", r)), null);
				return;
			}
			var me = ares_mx_reply.ToMailExchange(reply);
			ares_free_data(reply);
			cb.End(null, me);
		}

		public void Resolve(string host, AddressFamily addressFamily, Action<Exception, Hostent> callback)
		{
			switch (addressFamily) {
			case AddressFamily.InterNetwork:
			case AddressFamily.InterNetworkV6:
				var cb = new AresCallback<Hostent>(callback);
				if (addressFamily == AddressFamily.InterNetwork) {
					ares_query(channel, host, 1, 1, Callback4, cb.Handle);
				} else {
					ares_query(channel, host, 1, 28, Callback6, cb.Handle);
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

		public void ResolveMx(string host, Action<Exception, MailExchange[]> callback)
		{
			AresCallback<MailExchange[]> cb = new AresCallback<MailExchange[]>(callback);
			ares_query(channel, host, 1, 15, CallbackMx, cb.Handle);
		}
	}
}

