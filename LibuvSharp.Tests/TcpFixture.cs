using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace LibuvSharp.Tests
{
	public class TcpFixture : Fixture
	{
		[Fact]
		public void Simple()
		{
			SimpleTest(Default.IPv4);
			SimpleTest(Default.IPv6);
		}

		public void SimpleTest(IPEndPoint ep)
		{
			Default.SimpleTest<IPEndPoint, TcpListener, Tcp>(ep);

			Loop.Current.Run(async () => await Default.SimpleTestAsync<IPEndPoint, TcpListener, Tcp>(ep));
		}

		[Fact]
		public void Stress()
		{
			StressTest(Default.IPv4);
			StressTest(Default.IPv6);
		}

		public void StressTest(IPEndPoint ep)
		{
			Default.StressTest<IPEndPoint, TcpListener, Tcp>(ep);
		}

		[Fact]
		public void OneSideClose()
		{
			OneSideCloseTest(Default.IPv4);
			OneSideCloseTest(Default.IPv6);
		}

		public void OneSideCloseTest(IPEndPoint ep)
		{
			Default.OneSideCloseTest<IPEndPoint, TcpListener, Tcp>(ep);
		}

		[Fact]
		public void TakenPort()
		{
			var ep = Default.IPv4;

			TcpListener s1 = new TcpListener();
			TcpListener s2 = new TcpListener();

			s1.Bind(ep);
			s1.Listen();
			s2.Bind(ep);

			Assert.Throws<UVException>(() => s2.Listen());

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
			var ep = Default.IPv4;
			int port = ep.Port;
			var ip = ep.Address;
			var ipstr = ip.ToString();

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

			Loop.Current.Run();
		}

		[Fact]
		public void RemoteAndLocalAddress()
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

			Loop.Current.Run();

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

