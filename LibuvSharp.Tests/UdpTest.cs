using System;
using System.Text;
using System.Net;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class UdpTest
	{
		[Test]
		public static void Run()
		{
			Run(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));
			Run(new IPEndPoint(IPAddress.Parse("::1"), 8000));
		}

		public static void Run(IPEndPoint ep)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			Udp client = new Udp();
			Udp server = new Udp();

			server.Bind(ep);
			server.Receive(Encoding.ASCII, (str, rinfo) => {
				Assert.AreEqual(str, "PING");
				sv_recv_cb_called++;
				server.Send(rinfo, Encoding.ASCII.GetBytes("PONG"), (s) => {
					sv_send_cb_called++;
					server.Close(() => { close_cb_called++; });
				});
			});

			client.Send(ep, Encoding.ASCII.GetBytes("PING"), (s) => {
				cl_send_cb_called++;
				client.Receive(Encoding.ASCII, (str, rinfo) => {
					Assert.AreEqual(str, "PONG");
					cl_recv_cb_called++;
					client.Close(() => { close_cb_called++; });
				});
			});


			Assert.AreEqual(0, close_cb_called);
			Assert.AreEqual(0, cl_send_cb_called);
			Assert.AreEqual(0, cl_recv_cb_called);
			Assert.AreEqual(0, sv_send_cb_called);
			Assert.AreEqual(0, sv_recv_cb_called);

			Loop.Default.Run();

			Assert.AreEqual(2, close_cb_called);
			Assert.AreEqual(1, cl_send_cb_called);
			Assert.AreEqual(1, cl_recv_cb_called);
			Assert.AreEqual(1, sv_send_cb_called);
			Assert.AreEqual(1, sv_recv_cb_called);


#if DEBUG
			Assert.AreEqual(1, UV.PointerCount);
#endif
		}

		[Test]
		public static void NotNullUdp()
		{
			var u = new Udp();
			Action<bool> cb = (_) => { };
			string ipstr = "127.0.0.1";
			var ip = IPAddress.Parse(ipstr);
			var ep = new IPEndPoint(ip, 7999);

			// constructor
			Assert.Throws<ArgumentNullException>(() => new Udp(null));

			// bind
			Assert.Throws<ArgumentNullException>(() => u.Bind(null));
			Assert.Throws<ArgumentNullException>(() => u.Bind(null as string, 0));
			Assert.Throws<ArgumentNullException>(() => u.Bind(null as IPAddress, 0));

			// receive
			Assert.Throws<ArgumentNullException>(() => u.Receive(null));
			Assert.Throws<ArgumentNullException>(() => u.Receive(Encoding.ASCII, null));
			Assert.Throws<ArgumentNullException>(() => u.Receive(null, (_, __) => { }));

			// send
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }, 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }, cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }, 0, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[], 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[], cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[], 0, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }, 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }, cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }, 0, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[], 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[], cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[], 0, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }, 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }, cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }, 0, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[], 0));
			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[], cb));
			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[], 0, cb));

			u.Close();

			Loop.Default.RunOnce();
		}
	}
}

