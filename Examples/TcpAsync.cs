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
		public static async Task Server()
		{
			try {
				using (var server = new TcpListener()) {
					server.Bind(Default.IPEndPoint);
					server.Listen();

					using (var client = await server.AcceptAsync()) {

						client.Write("Hello World!");
						var str = await client.ReadStringAsync();
						Console.WriteLine("From Client: {0}", str);
					}
				}
			} catch (Exception e) {
				Console.WriteLine("Server Exception:");
				Console.WriteLine(e);
			}
		}

		public static async Task Client()
		{
			try {
				using (var client = new Tcp()) {
					await client.ConnectAsync(Default.IPEndPoint);

					client.Write("Labas Pasauli!");
					var str = await client.ReadStringAsync();
					Console.WriteLine("From Server: {0}", str);
				}
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
