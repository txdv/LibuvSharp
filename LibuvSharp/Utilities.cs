using System;
using System.Threading.Tasks;

namespace LibuvSharp.Utilities
{
	public static class UtilitiesExtensions
	{
		public static Task PumpAsync<T>(this IUVStream<T> readStream, IUVStream<T> writeStream)
		{
			return HelperFunctions.Wrap(writeStream, readStream.Pump);
		}

		public static void Pump<T>(this IUVStream<T> readStream, IUVStream<T> writeStream)
		{
			Pump(readStream, writeStream, (Action<Exception>)null);
		}

		public static void Pump<T>(this IUVStream<T> readStream, IUVStream<T> writeStream, Action<Exception> callback)
		{
			readStream.Pump(writeStream, (ex1, ex2) => {
				if (callback != null) {
					callback(ex1 ?? ex2);
				}
			});
		}

		public static void Pump<T>(this IUVStream<T> readStream, IUVStream<T> writeStream, Action<Exception, Exception> callback)
		{
			bool pending = false;
			bool done = false;

			Action<Exception, Exception> call = (ex1, ex2) => {
				if (done) {
					return;
				}
				done = true;
				if (callback != null) {
					callback(ex1, ex2);
				}
			};

			readStream.Data += ((data) => {
				writeStream.Write(data, null);
				if (writeStream.WriteQueueSize > 0) {
					pending = true;
					readStream.Pause();
				}
			});

			writeStream.Drain += () => {
				if (pending) {
					pending = false;
					readStream.Resume();
				}
			};

			readStream.Complete += () => {
				writeStream.Shutdown((ex) => call(ex, null));
			};

			readStream.Error += (ex) => call(ex, null);
			readStream.Error += (ex) => call(null, ex);

			readStream.Resume();
		}
	}
}

