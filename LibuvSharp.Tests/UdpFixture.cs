using System;
using System.Text;
using System.Net;
using Xunit;

namespace LibuvSharp.Tests
{
	public class UdpFixture
	{
		[Fact]
		public void Run()
		{
			RunTest(Default.IPv4.IPEndPoint);
			RunTest(Default.IPv6.IPEndPoint);
		}

		public void RunTest(IPEndPoint ep)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			Udp client = new Udp();
			Udp server = new Udp();

			server.Bind(ep);
			var serverBuffer = new ArraySegment<byte>(new byte[1024]);
			server.Receive(serverBuffer, (ex, msg) => {
				var data = msg.Payload;
				var str = Encoding.ASCII.GetString(data.Array, data.Offset, data.Count);
				Assert.Equal(str, "PING");
				sv_recv_cb_called++;
				server.Send(msg.EndPoint, Encoding.ASCII.GetBytes("PONG"), (s) => {
					sv_send_cb_called++;
					server.Close(() => close_cb_called++);
				});
			});

			client.Send(ep, Encoding.ASCII.GetBytes("PING"), (s) => {
				cl_send_cb_called++;
				var clientBuffer = new ArraySegment<byte>(new byte[1024]);
				client.Receive(clientBuffer, (ex, msg) => {
					var data = msg.Payload;
					var str = Encoding.ASCII.GetString(data.Array, data.Offset, data.Count);
					Assert.Equal(str, "PONG");
					cl_recv_cb_called++;
					client.Close(() => close_cb_called++);
				});
			});


			Assert.Equal(0, close_cb_called);
			Assert.Equal(0, cl_send_cb_called);
			Assert.Equal(0, cl_recv_cb_called);
			Assert.Equal(0, sv_send_cb_called);
			Assert.Equal(0, sv_recv_cb_called);

			Loop.Default.Run();

			Assert.Equal(2, close_cb_called);
			Assert.Equal(1, cl_send_cb_called);
			Assert.Equal(1, cl_recv_cb_called);
			Assert.Equal(1, sv_send_cb_called);
			Assert.Equal(1, sv_recv_cb_called);


#if DEBUG
			Assert.Equal(1, UV.PointerCount);
#endif
		}

		[Fact]
		public void NotNullUdp()
		{
			NotNullUdpTest(Default.IPv4.IPEndPoint);
			NotNullUdpTest(Default.IPv6.IPEndPoint);
		}

		public void NotNullUdpTest(IPEndPoint ep)
		{
			var u = new Udp();
			Action<Exception> cb = (_) => { };

			string ipstr = ep.Address.ToString();
			var ip = ep.Address;

			// constructor
			Assert.Throws<ArgumentNullException>(() => new Udp(null));

			// bind
			Assert.Throws<ArgumentNullException>(() => u.Bind(null));
			Assert.Throws<ArgumentNullException>(() => u.Bind(null as string, 0));
			Assert.Throws<ArgumentNullException>(() => u.Bind(null as IPAddress, 0));

			// send
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPEndPoint, new byte[] { }, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ep, null as byte[], cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as string, 0, new byte[] { }, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ipstr, 0, null as byte[], cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }));
			Assert.Throws<ArgumentNullException>(() => u.Send(null as IPAddress, 0, new byte[] { }, cb));

			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[]));
			Assert.Throws<ArgumentNullException>(() => u.Send(ip, 0, null as byte[], cb));

			u.Close();

			Loop.Default.RunOnce();
		}
	}
}

