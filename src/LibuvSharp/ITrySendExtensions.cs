using System;
using System.Net;

namespace LibuvSharp
{
	public static class ITrySendExtensions
	{
		#region IPAddress string

		public static int TrySend<TMessage>(this ITrySend<TMessage> sender, string ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(IPAddress.Parse(ipAddress), port, data, 0, data.Length);
		}

		public static int TrySend<TMessage>(this ITrySend<TMessage> sender, string ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(IPAddress.Parse(ipAddress), port, new ArraySegment<byte>(data, index, count));
		}

		#endregion

		#region IPAddress

		public static int TrySend<TMessage>(this ITrySend<TMessage> sender, IPAddress ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(ipAddress, port, data, 0, data.Length);
		}

		public static int TrySend<TMessage>(this ITrySend<TMessage> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(data, index, count));
		}

		#endregion

		#region TEndPoint

		public static int TrySend<TMessage, TEndPoint>(this ITrySend<TMessage> sender, TEndPoint endPoint, byte[] data)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(endPoint, data, 0, data.Length);
		}

		public static int TrySend<TMessage, TEndPoint>(this ITrySend<TMessage> sender, TEndPoint endPoint, byte[] data, int index, int count)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			return sender.TrySend(endPoint, new ArraySegment<byte>(data, index, count));
		}

		#endregion

		#region TMessage

		public static int TrySend<TMessage, TPayload>(this ITrySend<TMessage> sender, string ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(IPAddress.Parse(ipAddress), port, payload);
		}

		public static int TrySend<TMessage, TPayload>(this ITrySend<TMessage> sender, IPAddress ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			return sender.TrySend(new IPEndPoint(ipAddress, port), payload);
		}

		#endregion

		public static int TrySend<TMessage, TEndPoint, TPayload>(this ITrySend<TMessage> sender, TEndPoint endPoint, TPayload payload)
			where TMessage : IMessage<TEndPoint, TPayload>, new()
		{
			return sender.TrySend(new TMessage() { EndPoint = endPoint, Payload = payload });
		}
	}
}

