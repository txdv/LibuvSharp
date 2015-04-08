using System;
using System.Net;

namespace LibuvSharp
{
	public static class IMessageSenderExtensions
	{
		#region IPAddress string

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(IPAddress.Parse(ipAddress), port, data, 0, data.Length, callback);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(IPAddress.Parse(ipAddress), port, new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region IPAddress

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(ipAddress, port, data, 0, data.Length, callback);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region TEndPoint

		public static void Send<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data, Action<Exception> callback)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(endPoint, data, 0, data.Length, callback);
		}

		public static void Send<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(endPoint, data, null);
		}

		public static void Send<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data, int index, int count, Action<Exception> callback)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(endPoint, new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data, int index, int count)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			sender.Send(endPoint, data, index, count, null);
		}

		#endregion

		#region TMessage

		public static void Send<TMessage, TPayload>(this IMessageSender<TMessage> sender, string ipAddress, int port, TPayload payload, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(IPAddress.Parse(ipAddress), port, payload, callback);
		}

		public static void Send<TMessage, TPayload>(this IMessageSender<TMessage> sender, string ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(ipAddress, port, payload, null);
		}

		public static void Send<TMessage, TPayload>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, TPayload payload, Action<Exception> callback)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(new IPEndPoint(ipAddress, port), payload, callback);
		}

		public static void Send<TMessage, TPayload>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			sender.Send(ipAddress, port, payload, null);
		}

		#endregion

		public static void Send<TMessage, TEndPoint, TPayload>(this IMessageSender<TMessage> sender, TEndPoint endPoint, TPayload payload, Action<Exception> callback)
			where TMessage : IMessage<TEndPoint, TPayload>, new()
		{
			sender.Send(new TMessage() { EndPoint = endPoint, Payload = payload }, callback);
		}

		public static void Send<TMessage, TEndPoint, TPayload>(this IMessageSender<TMessage> sender, TEndPoint endPoint, TPayload payload)
			where TMessage : IMessage<TEndPoint, TPayload>, new()
		{
			sender.Send<TMessage, TEndPoint, TPayload>(endPoint, payload, null);
		}
	}
}

