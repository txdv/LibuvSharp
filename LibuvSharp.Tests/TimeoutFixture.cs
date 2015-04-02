using System;
using System.Net;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace LibuvSharp.Tests
{
	public class TimeoutFixture
	{
		[Fact]
		public void TimeoutsWork()
		{
			WorksWith<IPEndPoint, TcpListener, Tcp>(Default.IPv4.IPEndPoint);
			WorksWith<IPEndPoint, TcpListener, Tcp>(Default.IPv6.IPEndPoint);

			WorksWithAsync<IPEndPoint, TcpListener, Tcp>(Default.IPv4.IPEndPoint);
			WorksWithAsync<IPEndPoint, TcpListener, Tcp>(Default.IPv6.IPEndPoint);
		}

		public static void WorksWith<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, IDisposable, new()
			where TClient : IUVStream, IConnectable<TClient, TEndPoint>, IHandle, IDisposable, new()
		{
			var server = new TListener();
			server.Unref();
			server.Bind(endPoint);
			server.Listen();
			server.Connection += () => server.Accept().Shutdown();

			int callbacks = 0;

			var failClient = new TClient();
			Timeout.In<TEndPoint>(TimeSpan.FromTicks(1), failClient.Connect)(endPoint, (exception) => {
				Assert.IsType<TimeoutException>(exception);
				failClient.Dispose();
				callbacks++;
			});

			var successClient = new TClient();
			Timeout.In<TEndPoint>(TimeSpan.FromMilliseconds(100), successClient.Connect)(endPoint, (exception) => {
				Assert.Equal<Exception>(null, exception);
				successClient.Dispose();
				callbacks++;
			});

			Loop.Default.Run();

			server.Dispose();

			Assert.Equal<int>(2, callbacks);
		}

		public static void WorksWithAsync<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, IDisposable, new()
			where TClient : IUVStream, IConnectable<TClient, TEndPoint>, IHandle, IDisposable, new()
		{
			Loop.Default.Run(async () => {
				using (var server = new TListener()) {
					server.Unref();
					server.Bind(endPoint);
					server.Listen();
					server.Connection += () => server.Accept().Shutdown();

					int callbacks = 0;

					using (var failClient = new TClient()) {
						try {
							await failClient.ConnectAsync(endPoint, TimeSpan.FromTicks(1));
						} catch (TimeoutException) {
							callbacks++;
						}
					}

					using (var successClient = new TClient())
					await successClient.ConnectAsync(endPoint, TimeSpan.FromMilliseconds(100));

					callbacks++;

					Assert.Equal<int>(2, callbacks);
				}
			});
		}
	}
}

