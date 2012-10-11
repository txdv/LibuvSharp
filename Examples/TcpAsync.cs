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
		static IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8080);

		public static async Task Server()
		{
			try {
				var server = new ListenerAsync<TcpListener, Tcp>(new TcpListener());
				server.Listener.Bind(ep);
				server.Listener.Listen();

				var client = await server.AcceptStreamAsync();

				client.Write(Encoding.ASCII, "Hello World!");
				var str = await client.ReadStringAsync();
				Console.WriteLine("From Client: {0}", str);

				client.Shutdown();
				server.Listener.Close();
			} catch (Exception e) {
				Console.WriteLine("Server Exception:");
				Console.WriteLine(e);
			}
		}

		public static async Task Client()
		{
			try {
				var client = new Tcp();
				await client.ConnectAsync(ep);

				client.Write(Encoding.ASCII, "Labas Pasauli!");
				var str = await client.ReadStringAsync();
				Console.WriteLine("From Server: {0}", str);

				client.Shutdown();
			} catch (Exception e) {
				Console.WriteLine("Client Exception:");
				Console.WriteLine(e);
			}
		}

		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				Console.WriteLine("Starting example.");
				await Task.WhenAll(Server(), Client());
				Console.WriteLine("All finished.");
			});
		}
	}
}
