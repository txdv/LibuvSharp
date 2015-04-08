using System;
using System.Net;
using System.Threading.Tasks;
using LibuvSharp;

namespace LibuvSharp.Threading.Tasks
{
	public static class IMessageSenderExtensions
	{
		public static Task SendAsync<TEndPoint>(this IDatagram<TEndPoint> sender, TEndPoint endPoint, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap(endPoint, data, sender.Send);
		}

		#region IPAddress string

		public static Task SendAsync(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data)
		{
			return HelperFunctions.Wrap(ipAddress, port, data, sender.Send);
		}

		public static Task SendAsync(this IDatagram<IPEndPoint> sender, string ipAddress, int port, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap(ipAddress, port, data, index, count, sender.Send);
		}

		#endregion

		#region IPAddress

		public static Task SendAsync(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data)
		{
			return HelperFunctions.Wrap(ipAddress, port, data, sender.Send);
		}

		public static Task SendAsync(this IDatagram<IPEndPoint> sender, IPAddress ipAddress, int port, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap(ipAddress, port, data, index, count, sender.Send);
		}

		#endregion

		public static Task SendAsync<TMessage, TEndPoint>(this IDatagram<TEndPoint> sender, TMessage message)
			where TMessage : IMessage<TEndPoint, ArraySegment<byte>>

		{
			return HelperFunctions.Wrap(message.EndPoint, message.Payload, sender.Send);
		}

		#region Receive

		public static Task<UdpReceiveMessage> ReceiveAsync(this IDatagram<IPEndPoint> sender, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<ArraySegment<byte>, UdpReceiveMessage>(data, sender.Receive);
		}

		public static Task<UdpReceiveMessage> ReceiveAsync(this IDatagram<IPEndPoint> sender, byte[] data)
		{
			return HelperFunctions.Wrap<byte[], UdpReceiveMessage>(data, sender.Receive);
		}

		public static Task<UdpReceiveMessage> ReceiveAsync(this IDatagram<IPEndPoint> sender, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<byte[], int, int, UdpReceiveMessage>(data, index, count, sender.Receive);
		}

		#endregion
	}
}

