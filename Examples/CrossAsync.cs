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
				await client.ConnectAsync(Default.IPEndPoint);
				var stream = client.GetStream();
				var sw = new StreamWriter(stream);
				await sw.WriteAsync("Hello World from BCL!");
				await sw.FlushAsync();
			}
		}

		// BCL Tcp Server
		public static async Task BclServer()
		{
			var listener = new System.Net.Sockets.TcpListener(Default.IPEndPoint);
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
				await client.ConnectAsync(Default.IPEndPoint);
				await client.WriteAsync("Hello World from LibuvSharp!");
			}
		}

		// LibuvSharp Tcp Listener
		public static async Task LoopServer()
		{
			using (var server = new LibuvSharp.TcpListener()) {
				server.Bind(Default.IPEndPoint);
				server.Listen();

				using (var client = await server.AcceptAsync()) {
					Console.WriteLine("LibuvSharp: " + await client.ReadStringAsync());
				}
			}
		}
	}
}

