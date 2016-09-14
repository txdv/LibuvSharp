using System;
using System.Net;

namespace LibuvSharp
{
	public class UdpMessage : IMessage<IPEndPoint, ArraySegment<byte>>
	{
		public UdpMessage()
		{
		}

		public UdpMessage(IPEndPoint endPoint, byte[] payload)
			: this(endPoint, new ArraySegment<byte>(payload))
		{
		}

		public UdpMessage(IPEndPoint endPoint, byte[] payload, int offset, int count)
			: this(endPoint, new ArraySegment<byte>(payload, offset, count))
		{
		}

		public UdpMessage(IPAddress ipAddress, int port, byte[] payload)
			: this(new IPEndPoint(ipAddress, port), payload)
		{
		}

		public UdpMessage(IPAddress ipAddress, int port, byte[] payload, int offset, int count)
			: this(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(payload, offset, count))
		{
		}

		public UdpMessage(IPAddress ipAddress, int port, ArraySegment<byte> payload)
			: this(new IPEndPoint(ipAddress, port), payload)
		{
		}

		public UdpMessage(string ipAddress, int port, byte[] payload)
			: this(IPAddress.Parse(ipAddress), port, payload)
		{
		}

		public UdpMessage(string ipAddress, int port, ArraySegment<byte> payload)
			: this(IPAddress.Parse(ipAddress), port, payload)
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

