using System;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp.Utilities
{
	public static class UtilitiesExtensions
	{
		public static Task PumpAsync(this IUVStream readStream, ArraySegment<byte> buffer, IUVStream writeStream)
		{
			return HelperFunctions.Wrap(writeStream, buffer, readStream.Pump);
		}

		public static void Pump(this IUVStream readStream, IUVStream writeStream, ArraySegment<byte> buffer)
		{
			readStream.Pump(writeStream, buffer, null);
		}

		public static void Pump(this IUVStream readStream, IUVStream writeStream, ArraySegment<byte> buffer, Action<Exception> callback)
		{
			Action<Exception, int> OnData = null;

			OnData = (readException, nread) => {
				if (readException != null) {
					callback(readException);
					return;
				}

				if (nread == 0) {
					if (callback != null) {
						callback(null);
					}
					return;
				}

				writeStream.Write(new ArraySegment<byte>(buffer.Array, buffer.Offset, nread), (writeException) => {
					if (writeException != null) {
						callback(writeException);
						return;
					}

					readStream.Read(buffer, OnData);
				});
			};

			readStream.Read(buffer, OnData);
		}
	}
}

