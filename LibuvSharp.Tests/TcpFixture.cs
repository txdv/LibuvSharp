using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class TcpFixture
	{
		[TestCase]
		public static void Simple()
		{
			Simple(Default.IPv4.IPEndPoint);
			Simple(Default.IPv6.IPEndPoint);
		}

		public static void Simple(IPEndPoint ep)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new TcpListener();
			server.Bind(ep);
			server.Listen(() => {
				var socket = server.AcceptStream();
				socket.Resume();
				socket.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					socket.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });

					socket.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp client = new Tcp();
			client.Connect(ep, (_) => {
				client.Resume();
				client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
				client.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.AreEqual("PONG", str);
					client.Close(() => { close_cb_called++; });
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
			Assert.AreEqual(1, UV.PointerCount);
#endif
		}

		public static string Times(string str, int times)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < times; i++) {
				sb.Append(str);
			}
			return sb.ToString();
		}

		[TestCase]
		public static void Stress()
		{
			Stress(Default.IPv4.IPEndPoint);
			Stress(Default.IPv6.IPEndPoint);
		}

		public static void Stress(IPEndPoint ep)
		{
			for (int j = 0; j < 10; j++) {
				int times = 10;

				int close_cb_called = 0;
				int cl_send_cb_called = 0;
				int cl_recv_cb_called = 0;
				int sv_send_cb_called = 0;
				int sv_recv_cb_called = 0;

				var server = new TcpListener();
				server.Bind(ep);
				server.Listen(() => {
					var socket = server.AcceptStream();
					socket.Resume();
					socket.Read(Encoding.ASCII, (str) => {
						sv_recv_cb_called++;
						Assert.AreEqual(Times("PING", times), str);
						for (int i = 0; i < times; i++) {
							socket.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
						}
						socket.Close(() => { close_cb_called++; });
						server.Close(() => { close_cb_called++; });
					});
				});

				var client = new Tcp();
				client.Connect(ep, (_) => {
					client.Resume();
					for (int i = 0; i < times; i++) {
						client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
					}
					client.Read(Encoding.ASCII, (str) => {
						cl_recv_cb_called++;
						Assert.AreEqual(Times("PONG", times), str);
						client.Close(() => { close_cb_called++; });
					});
				});

				Assert.AreEqual(0, close_cb_called);
				Assert.AreEqual(0, cl_send_cb_called);
				Assert.AreEqual(0, cl_recv_cb_called);
				Assert.AreEqual(0, sv_send_cb_called);
				Assert.AreEqual(0, sv_recv_cb_called);

				Loop.Default.Run();

				Assert.AreEqual(3, close_cb_called);
				Assert.AreEqual(times, cl_send_cb_called);
				Assert.AreEqual(1, cl_recv_cb_called);
				Assert.AreEqual(times, sv_send_cb_called);
				Assert.AreEqual(1, sv_recv_cb_called);

#if DEBUG
				Assert.AreEqual(1, UV.PointerCount);
#endif
			}
		}

		[TestCase]
		public static void OneSideClose()
		{
			OneSideClose(Default.IPv4.IPEndPoint);
			OneSideClose(Default.IPv6.IPEndPoint);
		}

		public static void OneSideClose(IPEndPoint ep)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new TcpListener();
			server.Bind(ep);
			server.Listen(() => {
				var socket = server.AcceptStream();
				socket.Resume();
				socket.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					socket.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
					socket.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp client = new Tcp();
			client.Connect(ep, (_) => {
				client.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.AreEqual("PONG", str);
				});

				client.Complete += () => {
					close_cb_called++;
				};
				client.Resume();
				client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
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
			Assert.AreEqual(1, UV.PointerCount);
#endif
		}

		[Test]
		public static void TakenPort()
		{
			TcpListener s1 = new TcpListener();
			TcpListener s2 = new TcpListener();

			s1.Bind(IPAddress.Any, Default.Port);
			s1.Listen(() => {});
			s2.Bind(IPAddress.Any, Default.Port);

			Assert.Throws<SocketException>(() => s2.Listen(() => {}), "Address already in use");

			s1.Close();
			s2.Close();
		}

		public static Tcp Create()
		{
			return new Tcp();
		}

		[Test]
		public static void NotNullConnect()
		{
			Action<Exception> cb = (_) => { };
			int port = 8000;
			var ipstr = "127.0.0.1";
			var ip = IPAddress.Parse(ipstr);
			var ep = new IPEndPoint(ip, 8000);

			Assert.Throws<ArgumentNullException>(() => Create().Connect(null as IPEndPoint, cb));
			Assert.Throws<ArgumentNullException>(() => Create().Connect(null as string, port, cb));
			Assert.Throws<ArgumentNullException>(() => Create().Connect(null as IPAddress, port, cb));

			Assert.Throws<ArgumentNullException>(() => Create().Connect(ep, null));
			Assert.Throws<ArgumentNullException>(() => Create().Connect(ipstr, port, null));
			Assert.Throws<ArgumentNullException>(() => Create().Connect(ip, port, null));
		}

		[Test]
		public static void ConnectToNotListeningPort()
		{
			Tcp socket = new Tcp();
			socket.Connect("127.0.0.1", 7999, (e) => {
				Assert.IsNotNull(e);
			});

			Loop.Default.Run();
		}

		[Test]
		public static void PeerAndSockname()
		{
			Tcp client = null;
			Tcp server = null;

			bool called = false;
			var l = new TcpListener();
			l.Bind(IPAddress.Any, 8000);

			Action check = () => {
				if (client == null || server == null) {
					return;
				}

				Assert.AreEqual(client.Sockname, server.Peername);
				Assert.AreEqual(client.Peername, server.Sockname);
				Assert.AreEqual(server.Sockname.Port, 8000);

				client.Shutdown();
				server.Shutdown();
				l.Close();

				called = true;
			};

			l.Listen(() => {
				server = l.AcceptStream();
				check();
			});

			Tcp t = new Tcp();
			t.Connect("127.0.0.1", 8000, (e) => {
				client = t;
				check();
			});

			Loop.Default.Run();

			Assert.IsTrue(called);
		}

		[Test]
		public static void NotNullListener()
		{
			var t = new TcpListener();
			Assert.Throws<ArgumentNullException>(() => new TcpListener(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null as string, 8000));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null as IPAddress, 8000));
			Assert.Throws<ArgumentNullException>(() => t.Listen(null));
			t.Close();
		}
	}
}

