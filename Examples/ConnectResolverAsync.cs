using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace Test
{
	public static class TaskExtensions
	{
		public static Task<IEnumerable<TResult>> AsIEnumerable<TResult>(this Task<TResult[]> task)
		{
			return task.ContinueWith(t => t.Result as IEnumerable<TResult>);
		}
	}

	public static class ConnectResolverAsyncExtesions
	{
		public async static Task ConnectAsync(this Tcp tcp, Func<string, IPAddress[]> resolveFunction, DnsEndPoint dnsEndPoint)
		{
			await tcp.ConnectAsync(resolveFunction, dnsEndPoint.Host, dnsEndPoint.Port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Func<string, IPAddress[]> resolveFunction, string hostNameOrAddress, int port)
		{
			await tcp.ConnectAsync(tcp.Loop.QueueUserWorkItemAsync(() => resolveFunction(hostNameOrAddress)), port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Func<string, Task<IPAddress[]>> resolveFunctionAsync, DnsEndPoint dnsEndPoint)
		{
			await tcp.ConnectAsync(resolveFunctionAsync, dnsEndPoint.Host, dnsEndPoint.Port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Func<string, Task<IPAddress[]>> resolveFunctionAsync, string hostNameOrAddress, int port)
		{
			await tcp.ConnectAsync(resolveFunctionAsync(hostNameOrAddress), port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Func<string, Task<IEnumerable<IPAddress>>> resolveFunctionAsync, DnsEndPoint dnsEndPoint)
		{
			await tcp.ConnectAsync(resolveFunctionAsync, dnsEndPoint.Host, dnsEndPoint.Port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Func<string, Task<IEnumerable<IPAddress>>> resolveFunctionAsync, string hostNameOrAddress, int port)
		{
			await tcp.ConnectAsync(resolveFunctionAsync(hostNameOrAddress), port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Task<IPAddress[]> resolver, int port)
		{
			await tcp.ConnectAsync(resolver.AsIEnumerable(), port);
		}

		public async static Task ConnectAsync(this Tcp tcp, Task<IEnumerable<IPAddress>> resolver, int port)
		{
			await tcp.ConnectAsync((await resolver).Select(ip => new IPEndPoint(ip, port)));
		}

		public async static Task ConnectAsync(this Tcp tcp, IEnumerable<IPEndPoint> endPoints)
		{
			Exception lastException = null;
			bool connected = false;

			foreach (var endPoint in endPoints) {
				try {
					await tcp.ConnectAsync(endPoint);
					connected = true;
					break;
				} catch (Exception exception) {
					lastException = exception;
					continue;
				}
			}

			if (!connected) {
				throw lastException;
			}
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				var tcp = new Tcp();
				await tcp.ConnectAsync(Dns.GetHostAddressesAsync, "ip.bentkus.eu", 80);
				tcp.TryWrite("GET / HTTP/1.1\r\nHost: ip.bentkus.eu\r\n\r\n");
				var data = await tcp.ReadStructAsync();
				if (data.HasValue) {
					Console.WriteLine(Encoding.ASCII.GetString(data.Value.Array, data.Value.Offset, data.Value.Count));
				}
				Console.WriteLine("Connected!");
			});
		}
	}
}
