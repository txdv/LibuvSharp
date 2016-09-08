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
		class EmptyHandle : IHandle, IDisposable
		{
#region IHandle
			public void Ref()
			{
			}

			public void Unref()
			{
			}

			public bool HasRef {
				get {
					return true;
				}
			}

			public bool IsClosed {
				get {
					return true;
				}
			}

			public void Close(Action callback)
			{
				callback?.Invoke();
			}
#endregion

			public void Dispose()
			{
			}
		}

		class TimeoutListener : EmptyHandle, IListener<TimeoutClient>, IBindable<TimeoutListener, TimeSpan>
		{
#region IListener<TimeoutClient>
			public void Listen()
			{
			}

			public event Action Connection;

			public TimeoutClient Accept()
			{
				return null;
			}
#endregion

			public void Bind(TimeSpan endPoint)
			{
			}
		}

		class TimeoutClient : EmptyHandle, IConnectable<TimeoutClient, TimeSpan>, IUVStream<ArraySegment<byte>>
		{
			public void Connect(TimeSpan timeout, Action<Exception> callback)
			{
				UVTimer.Once(timeout, () => callback?.Invoke(null)).Unref();
			}

#region IUVStream<ArraySegment<<byte>>
			public Loop Loop { get; private set; }

			public event Action<Exception> Error;

			public bool Readable { get; } = true;

			public event Action Complete;

			public event Action<ArraySegment<byte>> Data;

			public void Resume()
			{
			}

			public void Pause()
			{
			}

			public bool Writeable { get; } = true;

			public event Action Drain;

			public long WriteQueueSize { get; } = 0;

			public void Write(ArraySegment<byte> data, Action<Exception> callback)
			{
			}

			public void Shutdown(Action<Exception> callback)
			{
			}
#endregion
		}

		[Fact]
		public void TimeoutsWork()
		{
			var ep = TimeSpan.FromSeconds(1);

			WorksWith<TimeSpan, TimeoutListener, TimeoutClient>(ep);
			WorksWith<TimeSpan, TimeoutListener, TimeoutClient>(ep);

			WorksWithAsync<TimeSpan, TimeoutListener, TimeoutClient>(ep);
			WorksWithAsync<TimeSpan, TimeoutListener, TimeoutClient>(ep);
		}

		public static void WorksWith<TEndPoint, TListener, TClient>(TEndPoint endPoint)
			where TListener : IListener<TClient>, IBindable<TListener, TEndPoint>, IHandle, IDisposable, new()
			where TClient : IUVStream<ArraySegment<byte>>, IConnectable<TClient, TEndPoint>, IHandle, IDisposable, new()
		{
			var server = new TListener();
			server.Unref();
			server.Bind(endPoint);
			server.Listen();
			server.Connection += () => server.Accept().Shutdown();

			int callbacks = 0;

			var failClient = new TClient();
			Timeout.In<TEndPoint>(TimeSpan.FromSeconds(0.75), failClient.Connect)(endPoint, (exception) => {
				Assert.IsType<TimeoutException>(exception);
				failClient.Dispose();
				callbacks++;
			});

			var successClient = new TClient();
			Timeout.In<TEndPoint>(TimeSpan.FromSeconds(1.25), successClient.Connect)(endPoint, (exception) => {
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
			where TClient : IUVStream<ArraySegment<byte>>, IConnectable<TClient, TEndPoint>, IHandle, IDisposable, new()
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
							await failClient.ConnectAsync(endPoint, TimeSpan.FromSeconds(0.75));
						} catch (TimeoutException) {
							callbacks++;
						}
					}

					using (var successClient = new TClient())
					await successClient.ConnectAsync(endPoint, TimeSpan.FromSeconds(1.25));

					callbacks++;

					Assert.Equal<int>(2, callbacks);
				}
			});
		}
	}
}

