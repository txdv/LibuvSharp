using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;
using Xunit;

namespace LibuvSharp.Tests
{
	public class IPInfo
	{
		public IPInfo(string str, int port)
		{
			IPAddressString = str;
			IPAddress = IPAddress.Parse(IPAddressString);
			Port = port;
			IPEndPoint = new IPEndPoint(IPAddress, Port);
		}

		public string IPAddressString { get; private set; }
		public IPAddress IPAddress { get; private set; }
		public int Port { get; private set; }
		public IPEndPoint IPEndPoint { get; private set; }
	}

	public static class Default
	{
		static Default()
		{
			Port = 8000;
			IPv4 = new IPInfo("127.0.0.1", Port);
			IPv6 = new IPInfo("::1", Port);

			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				Pipename = "testpipe";
			} else {
				Pipename = @"\\.\pipe\testpipe";
			}

			Directory = "./tmp";
			SecondDirectory = Directory + "2";

			File = "file";

			TestExecutable = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Test.exe");
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				TestArguments = new string[] { "/usr/bin/mono", TestExecutable };
			} else {
				TestArguments = new string[] { TestExecutable };
			}
		}

		public static int Port { get; private set; }
		public static IPInfo IPv4 { get; private set; }
		public static IPInfo IPv6 { get; private set; }

		public static string Pipename { get; private set; }

		public static string Directory { get; private set; }
		public static string SecondDirectory { get; private set; }

		public static string File { get; private set; }

		public static string[] TestArguments { get; private set; }
		public static string TestExecutable { get; private set; }

		private static string Times(string str, int times)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < times; i++) {
				sb.Append(str);
			}
			return sb.ToString();
		}

		public static void StressTest<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, new()
			where TClient : IUVStream, IConnectable<TClient, TEndPoint>, IHandle, new()
		{
			for (int j = 0; j < 10; j++) {
				int times = 10;

				int close_cb_called = 0;
				int cl_send_cb_called = 0;
				int cl_recv_cb_called = 0;
				int sv_send_cb_called = 0;
				int sv_recv_cb_called = 0;

				var server = new TListener();
				server.Bind(endPoint);
				server.Connection += () => {
					var socket = server.Accept();
					var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
					socket.Read(buffer, (ex, nread) => {
						var str = Encoding.Default.GetString(buffer.Take(nread));
						sv_recv_cb_called++;
						Assert.Equal(Times("PING", times), str);
						for (int i = 0; i < times; i++) {
							socket.Write(Encoding.ASCII, "PONG", (s) => sv_send_cb_called++);
						}
						socket.Close(() => close_cb_called++);
						server.Close(() => close_cb_called++);
					});
				};
				server.Listen();

				var client = new TClient();
				client.Connect(endPoint, (_) => {
					for (int i = 0; i < times; i++) {
						client.Write(Encoding.ASCII, "PING", (s) => cl_send_cb_called++);
					}
					//client.Read(Encoding.ASCII, (str) => {
					var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
					client.Read(buffer, (ex, nread) => {
						var str = Encoding.Default.GetString(buffer.Take(nread));
						cl_recv_cb_called++;
						Assert.Equal(Times("PONG", times), str);
						client.Close(() => close_cb_called++);
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
				Assert.Equal(1, UV.PointerCount);
				#endif
			}
		}

		public static void SimpleTest<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, new()
			where TClient : IUVStream, IConnectable<TClient, TEndPoint>, IHandle, new()
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new TListener();
			server.Bind(endPoint);
			server.Connection += () => {
				var pipe = server.Accept();

				var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
				pipe.Read(buffer, (exception, nread) => {
					var str = Encoding.Default.GetString(buffer.Take(nread));
					sv_recv_cb_called++;
					Assert.Equal("PING", str);
					pipe.Write(Encoding.ASCII, "PONG", (s) => sv_send_cb_called++);

					pipe.Close(() => close_cb_called++);
					server.Close(() => close_cb_called++);
				});
			};
			server.Listen();

			var client = new TClient();
			client.Connect(endPoint, (_) => {
				client.Write(Encoding.ASCII, "PING", (s) => cl_send_cb_called++);
				var buffer = new ArraySegment<byte>(new byte[8 * 1024]);

				client.Read(buffer, (exception, nread) => {
					var str = Encoding.Default.GetString(buffer.Take(nread));
					cl_recv_cb_called++;
					Assert.Equal("PONG", str);
					client.Close(() => close_cb_called++);
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
			Assert.Equal(1, UV.PointerCount);
			#endif
		}

		public static void OneSideCloseTest<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, new()
			where TClient : IUVStream, IConnectable<TClient, TEndPoint>, IHandle, new()
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			var server = new TListener();
			server.Bind(endPoint);
			server.Listen();
			server.Connection += () => {
				var socket = server.Accept();
				var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
				socket.Read(buffer, (exception, nread) => {
					var str = Encoding.Default.GetString(buffer.Take(nread));
					sv_recv_cb_called++;
					Assert.Equal("PING", str);
					socket.Write(Encoding.ASCII, "PONG", (s) => sv_send_cb_called++);
					socket.Close(() => close_cb_called++);
					server.Close(() => close_cb_called++);
				});
			};

			var client = new TClient();
			client.Connect(endPoint, (_) => {
				var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
				Action<Exception, int> OnData = null;
				OnData = (exception, nread) => {
					if (nread == 0) {
						close_cb_called++;
					} else {
						var str = Encoding.Default.GetString(buffer.Take(nread));
						cl_recv_cb_called++;
						Assert.Equal("PONG", str);
						client.Read(buffer, OnData);
					}
				};
				client.Read(buffer, OnData);
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
			Assert.Equal(1, UV.PointerCount);
			#endif
		}

		public static async Task SimpleTestServerAsync<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IBindable<TListener, TEndPoint>, IListener<TClient>, IDisposable, new()
			where TClient : IUVStream, IDisposable, new()
		{
			using (var server = new TListener()) {
				server.Bind(endPoint);
				server.Listen();
				using (var client = await server.AcceptAsync()) {
					var str = await client.ReadStringAsync();
					if (str != null && str == "PING") {
						client.Write("PONG");
						await client.ShutdownAsync();
					}
				}
			}
		}

		public static async Task SimpleTestClientAsync<TEndPoint, TClient>(TEndPoint endPoint)
			where TClient : IConnectable<TClient, TEndPoint>, IUVStream, IDisposable, new()
		{
			using (var client = new TClient()) {
				await client.ConnectAsync(endPoint);

				client.Write("PING");
				var str = await client.ReadStringAsync();
				if (str != null) {
					if (str != "PONG") {
						throw new Exception("Should be PONG");
					}
				} else {
					throw new Exception("Shouldn't be null");
				}
				await client.ShutdownAsync();
			}
		}

		public static async Task SimpleTestAsync<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IBindable<TListener, TEndPoint>, IListener<TClient>, IDisposable, new()
			where TClient : IConnectable<TClient, TEndPoint>, IUVStream, IDisposable, new()
		{
			await Task.WhenAll(
				SimpleTestServerAsync<TEndPoint, TListener, TClient>(endPoint),
				SimpleTestClientAsync<TEndPoint, TClient>(endPoint)
			);
		}
	}
}

