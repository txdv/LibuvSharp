using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace CrossAsync
{
	/*
		This example shows how the BCL class can be simply used in the LibuvSharp event loop.
	*/
	class MainClass
	{
		static IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);

		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				await Task.WhenAll(LoopServer(), LoopClient());
				await Task.WhenAll(LoopServer(), BclClient());
				await Task.WhenAll(BclServer(), BclClient());
				await Task.WhenAll(BclServer(), LoopClient());
			});
		}

		// BCL Tcp Client
		public static async Task BclClient()
		{
			using (var client = new TcpClient()) {
				await client.ConnectAsync(ipEndPoint);
				var stream = client.GetStream();
				var sw = new StreamWriter(stream);
				await sw.WriteAsync("Hello World from BCL!");
				await sw.FlushAsync();
			}
		}

		// BCL Tcp Server
		public static async Task BclServer()
		{
			var listener = new System.Net.Sockets.TcpListener(ipEndPoint);
			try {
				listener.Start();
				using (var client = await listener.AcceptTcpClientAsync()) {
					var sr = new StreamReader(client.GetStream());
					var message = await sr.ReadToEndAsync();
					Console.WriteLine("BCL: " + message);
				}
			} finally {
				listener.Stop();
			}
		}

		// LibuvSharp Tcp Client
		public static async Task LoopClient()
		{
			using (var client = new Tcp()) {
				await client.ConnectAsync(ipEndPoint);
				await client.WriteAsync("Hello World from LibuvSharp!");
			}
		}

		// LibuvSharp Tcp Listener
		public static async Task LoopServer()
		{
			using (var server = new LibuvSharp.TcpListener()) {
				server.Bind(ipEndPoint);
				server.Listen();

				using (var client = await server.AcceptAsync()) {
					var msg = await client.ReadStructAsync();
					Console.WriteLine("LibuvSharp: " + Encoding.Default.GetString(msg));
				}
			}
		}
	}
}

