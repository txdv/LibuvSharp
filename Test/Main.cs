using System;
using System.Net;
using System.Text;
using Libuv;

namespace Test
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			UdpTest.Run();

			/*
			new TcpServer(loop, (sock) => {
				//Console.WriteLine ("ASD");
				sock.Start();
			}).Listen(IPAddress.Parse("127.0.0.1"), 8000, 128);

			Tcp t = new Tcp(loop);
			t.Connect(IPAddress.Parse("127.0.0.1"), 8001, (stream) => {
				//Stream s = new Stream(req);
				stream.Start();
				stream.Write(Encoding.ASCII.GetBytes("Hello world!"));
			});
			 */
		}
	}
}
