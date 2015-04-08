using System;
using System.Net;

namespace LibuvSharp
{
	public static class ITrySendExtensions
	{
		#region IPAddress string

		public static int TrySend(this ITrySend<IPEndPoint> sender, string ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(IPAddress.Parse(ipAddress), port, data, 0, data.Length);
		}

		public static int TrySend<TMessage>(this ITrySend<IPEndPoint> sender, string ipAddress, int port, byte[] data, int index, int count)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(IPAddress.Parse(ipAddress), port, data, index, count);
		}

		#endregion

		#region IPAddress

		public static int TrySend(this ITrySend<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(ipAddress, port, data, 0, data.Length);
		}

		public static int TrySend(this ITrySend<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(data, index, count));
		}

		#endregion

		#region IPAddress

		public static int TrySend<TEndPoint>(this ITrySend<TEndPoint> sender, TEndPoint endPoint, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(endPoint, data, 0, data.Length);
		}

		public static int TrySend<TEndPoint>(this ITrySend<TEndPoint> sender, TEndPoint endPoint, byte[] data, int index, int count)
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(endPoint, "endPoint");
			return sender.TrySend(endPoint, new ArraySegment<byte>(data, index, count));
		}

		#endregion

		public static int TrySend<TEndPoint>(this ITrySend<TEndPoint> sender, IMessage<TEndPoint, ArraySegment<byte>> message)
		{
			return sender.TrySend(message.EndPoint, message.Payload);
		}
	}
}

