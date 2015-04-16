using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

namespace Test
{
	class MainClass
	{
		public static void Main(string[] args )
		{
			Loop.Default.Run(() => {
				var listener = new TcpListener();
				listener.Bind(Default.IPEndPoint);
				listener.Listen();
				var response = Encoding.Default.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 13\r\n\r\nHello World\r\n");
				listener.Connection += () => {
					var client = listener.Accept();
					client.Data += (ArraySegment<byte> obj) => {
						client.TryWrite(response);
						client.Shutdown();
					};
					client.Resume();
				};
			});
		}
	}
}