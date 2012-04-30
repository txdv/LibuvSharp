using System;
using Libuv;
using System.Net;
using System.Text;

using NUnit.Framework;

namespace Test
{
	[TestFixture]
	public class PipeTest
	{
		[TestCase]
		public static void Simple()
		{
			Simple("PipeTestSimple");
		}

		public static void Simple(string name)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new PipeListener();
			server.Bind(name);
			server.Listen((pipe) => {
				pipe.Resume();
				pipe.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });

					pipe.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Pipe.Connect(name, (client) => {
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
			Stress("PipeTestStress");
		}

		public static void Stress(string name)
		{
			for (int j = 0; j < 10; j++) {
				int times = 10;

				int close_cb_called = 0;
				int cl_send_cb_called = 0;
				int cl_recv_cb_called = 0;
				int sv_send_cb_called = 0;
				int sv_recv_cb_called = 0;

				var server = new PipeListener();
				server.Bind(name);
				server.Listen((pipe) => {
					pipe.Resume();
					pipe.Read(Encoding.ASCII, (str) => {
						sv_recv_cb_called++;
						Assert.AreEqual(Times("PING", times), str);
						for (int i = 0; i < times; i++) {
							pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
						}
						pipe.Close(() => { close_cb_called++; });
						server.Close(() => { close_cb_called++; });
					});
				});

				Pipe.Connect(name, (client) => {
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
			OneSideClose("PipeTestOneSidedClose");
		}

		public static void OneSideClose(string name)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new PipeListener();
			server.Bind(name);
			server.Listen((pipe) => {
				pipe.Resume();
				pipe.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.AreEqual("PING", str);
					pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
					pipe.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Pipe.Connect(name, (client) => {
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
	}
}

