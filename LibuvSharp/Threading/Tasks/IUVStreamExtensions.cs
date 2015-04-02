using System;
using System.Text;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IUVStreamExtensions
	{
		#region Read

		public static Task<int> ReadAsync(this IUVStream stream, ArraySegment<byte> buffer)
		{
			return HelperFunctions.Wrap<ArraySegment<byte>, int>(buffer, stream.Read);
		}

		public static Task<int> ReadAsync(this IUVStream stream, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<byte[], int, int, int>(data, index, count, stream.Read);
		}

		public static Task<int> ReadAsync(this IUVStream stream, byte[] data, int index)
		{
			return HelperFunctions.Wrap<byte[], int, int>(data, index, stream.Read);
		}

		public static Task<int> ReadAsync(this IUVStream stream, byte[] data)
		{
			return HelperFunctions.Wrap<byte[], int>(data, stream.Read);
		}
		#endregion

		#region WriteAsync

		public static Task WriteAsync(this IUVStream stream, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap(data, stream.Write);
		}

		public static Task WriteAsync(this IUVStream stream, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap(data, index, count, stream.Write);
		}

		public static Task WriteAsync(this IUVStream stream, byte[] data)
		{
			return HelperFunctions.Wrap(data, stream.Write);
		}

		public static Task<int> WriteAsync(this IUVStream stream, Encoding encoding, string text)
		{
			return HelperFunctions.Wrap<Encoding, string, int>(encoding, text, stream.Write);
		}

		public static Task<int> WriteAsync(this IUVStream stream, string text)
		{
			return HelperFunctions.Wrap<string, int>(text, stream.Write);
		}

		#endregion

		public static Task ShutdownAsync(this IUVStream stream)
		{
			return HelperFunctions.Wrap(stream.Shutdown);
		}
	}
}

