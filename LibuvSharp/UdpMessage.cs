using System;
using System.Net;

namespace LibuvSharp
{
	public class UdpMessage : IMessage<IPEndPoint, ArraySegment<byte>>
	{
		public UdpMessage()
		{
		}

		public UdpMessage(IPEndPoint endPoint, ArraySegment<byte> payload)
		{
			EndPoint = endPoint;
			Payload = payload;
		}

		public IPEndPoint EndPoint { get; set; }
		public ArraySegment<byte> Payload { get; set; }
	}
}

