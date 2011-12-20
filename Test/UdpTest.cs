using System;
using System.Text;
using System.Net;
using Libuv;

using NUnit.Framework;

namespace Test
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
				server.Send(rinfo, Encoding.ASCII.GetBytes("PONG"), () => {
					sv_send_cb_called++;
					server.Close(() => { close_cb_called++; });
				});
			});

			client.Send(ep, Encoding.ASCII.GetBytes("PING"), () => {
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
			Assert.AreEqual(0, UV.PointerCount);
#endif
		}
	}
}

