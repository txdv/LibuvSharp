using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace LibuvSharp.Tests
{
	public class TcpFixture
	{
		[Fact]
		public void Simple()
		{
			SimpleTest(Default.IPv4.IPEndPoint);
			SimpleTest(Default.IPv6.IPEndPoint);
		}

		public void SimpleTest(IPEndPoint ep)
		{
			Default.SimpleTest<IPEndPoint, TcpListener, Tcp>(ep);
		}

		[Fact]
		public void Stress()
		{
			StressTest(Default.IPv4.IPEndPoint);
			StressTest(Default.IPv6.IPEndPoint);
		}

		public void StressTest(IPEndPoint ep)
		{
			Default.StressTest<IPEndPoint, TcpListener, Tcp>(ep);
		}

		[Fact]
		public void OneSideClose()
		{
			OneSideCloseTest(Default.IPv4.IPEndPoint);
			OneSideCloseTest(Default.IPv6.IPEndPoint);
		}

		public void OneSideCloseTest(IPEndPoint ep)
		{
			Default.OneSideCloseTest<IPEndPoint, TcpListener, Tcp>(ep);
		}

		[Fact]
		public void TakenPort()
		{
			TcpListener s1 = new TcpListener();
			TcpListener s2 = new TcpListener();

			s1.Bind(IPAddress.Any, Default.Port);
			s1.Listen();
			s2.Bind(IPAddress.Any, Default.Port);

			Assert.Throws<SocketException>(() => s2.Listen());

			s1.Close();
			s2.Close();
		}

		public static Tcp Create()
		{
			return new Tcp();
		}

		[Fact]
		public void NotNullConnect()
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

		[Fact]
		public void ConnectToNotListeningPort()
		{
			Tcp socket = new Tcp();
			socket.Connect("127.0.0.1", 7999, (e) => {
				Assert.NotNull(e);
			});

			Loop.Default.Run();
		}

		[Fact]
		public void PeerAndSockname()
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

				Assert.Equal(client.LocalAddress, server.RemoteAddress);
				Assert.Equal(client.RemoteAddress, server.LocalAddress);
				Assert.Equal(server.LocalAddress.Port, 8000);

				client.Shutdown();
				server.Shutdown();
				l.Close();

				called = true;
			};

			l.Listen();
			l.Connection += () => {
				server = l.Accept();
				check();
			};

			Tcp t = new Tcp();
			t.Connect("127.0.0.1", 8000, (e) => {
				client = t;
				check();
			});

			Loop.Default.Run();

			Assert.True(called);
		}

		[Fact]
		public void NotNullListener()
		{
			var t = new TcpListener();
			Assert.Throws<ArgumentNullException>(() => new TcpListener(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null as string, 8000));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null as IPAddress, 8000));
			t.Close();
		}
	}
}

