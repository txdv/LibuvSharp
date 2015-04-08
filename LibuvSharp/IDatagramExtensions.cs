using System;
using System.Net;

namespace LibuvSharp
{
	public static class IDatagramExtensions
	{
		#region IPAddress string

		public static void Send(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(IPAddress.Parse(ipAddress), port, data, 0, data.Length, callback);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data)
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(IPAddress.Parse(ipAddress), port, data, index, count, callback);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data, int index, int count)
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region IPAddress

		public static void Send(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(ipAddress, port, data, 0, data.Length, callback);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data)
		{
			sender.Send(ipAddress, port, data, null);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			sender.Send(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
		{
			sender.Send(ipAddress, port, data, index, count, null);
		}

		#endregion

		#region TEndPoint

		public static void Send<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, byte[] data, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(endPoint, data, 0, data.Length, callback);
		}

		public static void Send<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, byte[] data)
		{
			sender.Send(endPoint, data, null);
		}

		public static void Send<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, byte[] data, int index, int count, Action<Exception> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			sender.Send(endPoint, new ArraySegment<byte>(data, index, count), callback);
		}

		public static void Send<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, byte[] data, int index, int count)
		{
			sender.Send(endPoint, data, index, count, null);
		}

		#endregion

		#region TMessage

		public static void Send<TMessage, TEndPoint>(this IDatagram<TEndPoint> sender, TMessage message, Action<Exception> callback)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>

		{
			sender.Send(message.EndPoint, message.Payload, callback);
		}

		public static void Send<TMessage, TEndPoint>(this IDatagram<TEndPoint> sender, TMessage message)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>

		{
			sender.Send(message.EndPoint, message.Payload, null);
		}

		#endregion

		public static void Send<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, ArraySegment<byte> data)
		{
			sender.Send(endPoint, data, null);
		}

		#region Receive

		public static void Receive<TEndPoint>(this IDatagram<TEndPoint> receiver, byte[] data, Action<Exception, UdpReceiveMessage> callback)
		{
			receiver.Receive(new ArraySegment<byte>(data), callback);
		}

		public static void Receive<TEndPoint>(this IDatagram<TEndPoint> receiver, byte[] data, int index, int count, Action<Exception, UdpReceiveMessage> callback)
		{
			receiver.Receive(new ArraySegment<byte>(data, index, count), callback);
		}

		#endregion
	}
}

