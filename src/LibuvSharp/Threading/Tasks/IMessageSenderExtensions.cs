using System;
using System.Net;
using System.Threading.Tasks;
using LibuvSharp;

namespace LibuvSharp.Threading.Tasks
{
	public static class IMessageSenderExtensions
	{
		public static Task SendAsync<TMessage>(this IMessageSender<TMessage> sender, TMessage message)
		{
			return HelperFunctions.Wrap(message, sender.Send);
		}

		#region IPAddress string

		public static Task SendAsync<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, data, sender.Send<TMessage>);
		}

		public static Task SendAsync<TMessage>(this IMessageSender<TMessage> sender, string ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, data, index, count, sender.Send<TMessage>);
		}

		#endregion

		#region IPAddress

		public static Task SendAsync<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, data, sender.Send<TMessage>);
		}

		public static Task SendAsync<TMessage>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
			where TMessage : IMessage<IPEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, data, index, count, sender.Send<TMessage>);
		}

		#endregion

		#region TEndPoint

		public static Task SendAsync<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(endPoint, data, sender.Send);
		}

		public static Task SendAsync<TMessage, TEndPoint>(this IMessageSender<TMessage> sender, TEndPoint endPoint, byte[] data, int index, int count)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>, new()
		{
			return HelperFunctions.Wrap(endPoint, data, index, count, sender.Send);
		}

		#endregion

		#region TMessage

		public static Task SendAsync<TMessage, TPayload>(this IMessageSender<TMessage> sender, string ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, payload, sender.Send);
		}

		public static Task SendAsync<TMessage, TPayload>(this IMessageSender<TMessage> sender, IPAddress ipAddress, int port, TPayload payload)
			where TMessage : IMessage<IPEndPoint, TPayload>, new()
		{
			return HelperFunctions.Wrap(ipAddress, port, payload, sender.Send);
		}

		#endregion

		public static Task SendAsync<TMessage, TEndPoint, TPayload>(this IMessageSender<TMessage> sender, TEndPoint endPoint, TPayload payload)
			where TMessage : IMessage<TEndPoint, TPayload>, new()
		{
			return HelperFunctions.Wrap(endPoint, payload, sender.Send);
		}
	}
}

