using System;
using Libuv;
using System.Net;
using System.Text;

using NUnit.Framework;

namespace Test
{
	[TestFixture]
	public class TcpTest
	{
		[TestCase]
		public static void Run()
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			Tcp server = new Tcp();
			server.Bind("127.0.0.1", 8000);
			server.Listen(128, (stream) => {
				stream.Start();
				stream.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					stream.Write(Encoding.ASCII, "PONG", () => { sv_send_cb_called++; });

					stream.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp client = new Tcp();
			client.Connect("127.0.0.1", 8000, (stream) => {
				stream.Start();
				stream.Write(Encoding.ASCII, "PING", () => { cl_send_cb_called++; });
				stream.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.AreEqual("PONG", str);
					stream.Close(() => { close_cb_called++; });
					//client.Close(() => { close_cb_called++; });
				});
			});

			Assert.AreEqual(0, close_cb_called);
			Assert.AreEqual(0, cl_send_cb_called);
			Assert.AreEqual(0, cl_recv_cb_called);
			Assert.AreEqual(0, sv_send_cb_called);
			Assert.AreEqual(0, sv_recv_cb_called);

			Loop.Default.Run();

			Assert.AreEqual(3, close_cb_called);
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

