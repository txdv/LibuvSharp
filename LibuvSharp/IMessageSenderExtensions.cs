using System;
using System.Net;

namespace LibuvSharp
{
	public static class IMessageSenderExtensions
	{
		#region IPAddress string

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data, Action<Exception> callback)
		{
			sender.Send(ipAddress, port, data, 0, callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data)
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data, int index, Action<Exception> callback)
		{
			sender.Send(ipAddress, port, data, index, data.Length - index, callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data, int index)
		{
			sender.Send(ipAddress, port, data, index, null);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(IPAddress.Parse(ipAddress), port, new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, string ipAddress, int port, byte[] data, int index, int count)
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region IPAddress

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data, Action<Exception> callback)
		{
			sender.Send(ipAddress, port, data, 0, callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data)
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data, int index, Action<Exception> callback)
		{
			sender.Send(ipAddress, port, data, index, data.Length - index, callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data, int index)
		{
			sender.Send(ipAddress, port, data, index, null);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send(this IMessageSender<IPEndPoint, ArraySegment<byte>> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region TEndPoint

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data, Action<Exception> callback)
		{
			sender.Send(endPoint, data, 0, callback);
		}

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data)
		{
			sender.Send(endPoint, data, null);
		}

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data, int index, Action<Exception> callback)
		{
			sender.Send(endPoint, data, index, data.Length - index, callback);
		}

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data, int index)
		{
			sender.Send(endPoint, data, index, null);
		}

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(endPoint, new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send<TEndPoint>(this IMessageSender<TEndPoint, ArraySegment<byte>> sender, TEndPoint endPoint, byte[] data, int index, int count)
		{
			sender.Send(endPoint, data, index, count, null);
		}

		#endregion

		#region TMEssage

		public static void Send<TMessage>(this IMessageSender<IPEndPoint, TMessage> sender, string ipAddress, int port, TMessage message, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(IPAddress.Parse(ipAddress), port, message, callback);
		}

		public static void Send<TMessage>(this IMessageSender<IPEndPoint, TMessage> sender, string ipAddress, int port, TMessage message)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(ipAddress, port, message, null);
		}

		public static void Send<TMessage>(this IMessageSender<IPEndPoint, TMessage> sender, IPAddress ipAddress, int port, TMessage message, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(new IPEndPoint(ipAddress, port), message, callback);
		}

		public static void Send<TMessage>(this IMessageSender<IPEndPoint, TMessage> sender, IPAddress ipAddress, int port, TMessage message)
		{
			sender.Send(ipAddress, port, message, null);
		}

		#endregion

		#region TEndPoint, TMessage

		public static void Send<TEndPoint, TMessage>(this IMessageSender<TEndPoint, TMessage> sender, TEndPoint endPoint, TMessage message)
		{
			sender.Send(endPoint, message, null);
		}

		#endregion
	}
}

