using System;

namespace LibuvSharp.Utilities
{
	public static class UtilitiesExtensions
	{
		public static void Pump(this IUVStream readStream, IUVStream writeStream)
		{
			Pump(readStream, writeStream, null);
		}

		public static void Pump(this IUVStream readStream, IUVStream writeStream, Action<Exception, Exception> callback)
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

			readStream.Data += ((bb) => {
				writeStream.Write(bb.Array, bb.Offset, bb.Count);
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
				writeStream.Shutdown(() => call(null, null));
			};

			readStream.Error += (ex) => call(ex, null);
			readStream.Error += (ex) => call(null, ex);

			readStream.Resume();
		}
	}
}

