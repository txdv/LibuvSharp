using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class TcpTest
	{
		[TestCase]
		public static void Simple()
		{
			Simple(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));
			Simple(new IPEndPoint(IPAddress.Parse("::1"), 8000));
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
			server.Listen((socket) => {
				socket.Resume();
				socket.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					socket.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });

					socket.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp.Connect(Loop.Default, ep, (_, client) => {
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
			Stress(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));
			Stress(new IPEndPoint(IPAddress.Parse("::1"), 8000));
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
				server.Listen((socket) => {
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

				Tcp.Connect(ep, (_, client) => {
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
			OneSideClose(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));
			OneSideClose(new IPEndPoint(IPAddress.Parse("::1"), 8000));
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
			server.Listen((socket) => {
				socket.Resume();
				socket.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					socket.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
					socket.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp.Connect(ep, (_, client) => {
				client.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.AreEqual("PONG", str);
				});

				client.EndOfStream += () => {
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

			s1.Bind(IPAddress.Any, 8000);
			s1.Listen((_) => {});
			s2.Bind(IPAddress.Any, 8000);

			Assert.Throws<SocketException>(() => s2.Listen((_) => {}), "Address already in use");

			s1.Close();
			s2.Close();
		}

		[Test]
		public static void ConnectToNotListeningPort()
		{
			Tcp.Connect("127.0.0.1", 7999, (error, socket) => {
				Assert.Throws<SocketException>(() => { throw error; }, "Connection refused");
			});
		}
	}
}

