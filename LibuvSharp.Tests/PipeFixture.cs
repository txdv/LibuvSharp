using System;
using System.Net;
using System.Text;
using Xunit;

namespace LibuvSharp.Tests
{
	public class PipeFixture
	{
		[Fact]
		public void Simple()
		{
			SimpleTest(Default.Pipename);
		}

		public void SimpleTest(string name)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new PipeListener();
			server.Bind(name);
			server.Connection += () => {
				var pipe = server.Accept();
				pipe.Resume();
				pipe.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.Equal("PING", str);
					pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });

					pipe.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			};
			server.Listen();

			Pipe client = new Pipe();
			client.Connect(name, (_) => {
				client.Resume();
				client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
				client.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.Equal("PONG", str);
					client.Close(() => { close_cb_called++; });
				});
			});

			Assert.Equal(0, close_cb_called);
			Assert.Equal(0, cl_send_cb_called);
			Assert.Equal(0, cl_recv_cb_called);
			Assert.Equal(0, sv_send_cb_called);
			Assert.Equal(0, sv_recv_cb_called);

			Loop.Default.Run();

			Assert.Equal(3, close_cb_called);
			Assert.Equal(1, cl_send_cb_called);
			Assert.Equal(1, cl_recv_cb_called);
			Assert.Equal(1, sv_send_cb_called);
			Assert.Equal(1, sv_recv_cb_called);

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

		[Fact]
		public void Stress()
		{
			StressTest(Default.Pipename);
		}

		public void StressTest(string name)
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
				server.Connection += () => {
					var pipe = server.Accept();
					pipe.Resume();
					pipe.Read(Encoding.ASCII, (str) => {
						sv_recv_cb_called++;
						Assert.Equal(Times("PING", times), str);
						for (int i = 0; i < times; i++) {
							pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
						}
						pipe.Close(() => { close_cb_called++; });
						server.Close(() => { close_cb_called++; });
					});
				};
				server.Listen();

				Pipe client = new Pipe();
				client.Connect(name, (_) => {
					client.Resume();
					for (int i = 0; i < times; i++) {
						client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
					}
					client.Read(Encoding.ASCII, (str) => {
						cl_recv_cb_called++;
						Assert.Equal(Times("PONG", times), str);
						client.Close(() => { close_cb_called++; });
					});
				});

				Assert.Equal(0, close_cb_called);
				Assert.Equal(0, cl_send_cb_called);
				Assert.Equal(0, cl_recv_cb_called);
				Assert.Equal(0, sv_send_cb_called);
				Assert.Equal(0, sv_recv_cb_called);

				Loop.Default.Run();

				Assert.Equal(3, close_cb_called);
				Assert.Equal(times, cl_send_cb_called);
				Assert.Equal(1, cl_recv_cb_called);
				Assert.Equal(times, sv_send_cb_called);
				Assert.Equal(1, sv_recv_cb_called);

#if DEBUG
				Assert.AreEqual(1, UV.PointerCount);
#endif
			}
		}

		[Fact]
		public void OneSideClose()
		{
			OneSideCloseTest(Default.Pipename);
		}

		public void OneSideCloseTest(string name)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new PipeListener();
			server.Bind(name);
			server.Connection += () => {
				var pipe = server.Accept();
				pipe.Resume();
				pipe.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Assert.Equal("PING", str);
					pipe.Write(Encoding.ASCII, "PONG", (s) => { sv_send_cb_called++; });
					pipe.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			};
			server.Listen();

			Pipe client = new Pipe();
			client.Connect(name, (_) => {
				client.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Assert.Equal("PONG", str);
				});

				client.Complete += () => {
					close_cb_called++;
				};
				client.Resume();
				client.Write(Encoding.ASCII, "PING", (s) => { cl_send_cb_called++; });
			});

			Assert.Equal(0, close_cb_called);
			Assert.Equal(0, cl_send_cb_called);
			Assert.Equal(0, cl_recv_cb_called);
			Assert.Equal(0, sv_send_cb_called);
			Assert.Equal(0, sv_recv_cb_called);

			Loop.Default.Run();

			Assert.Equal(3, close_cb_called);
			Assert.Equal(1, cl_send_cb_called);
			Assert.Equal(1, cl_recv_cb_called);
			Assert.Equal(1, sv_send_cb_called);
			Assert.Equal(1, sv_recv_cb_called);

#if DEBUG
			Assert.AreEqual(1, UV.PointerCount);
#endif
		}

		[Fact]
		public void ConnectToNotListeningFile()
		{
			Pipe pipe = new Pipe();
			pipe.Connect("NOT_EXISTING", (e) => {
				Assert.NotNull(e);
				Assert.Equal(e.GetType(), typeof(System.IO.FileNotFoundException));
			});
			Loop.Default.Run();
		}

		[Fact]
		public void NotNullListener()
		{
			var t = new PipeListener();
			Assert.Throws<ArgumentNullException>(() => new PipeListener(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null));
			t.Close();
		}
	}
}

