using System;
using System.Net;

namespace LibuvSharp
{
	public class UdpReceiveMessage : UdpMessage
	{
		public UdpReceiveMessage()
		{
		}

		public UdpReceiveMessage(IPEndPoint endPoint, byte[] payload)
			: this(endPoint, new ArraySegment<byte>(payload))
		{
		}

		public UdpReceiveMessage(IPEndPoint endPoint, byte[] payload, int offset, int count)
			: this(endPoint, new ArraySegment<byte>(payload, offset, count))
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, byte[] payload)
			: this(new IPEndPoint(ipAddress, port), payload)
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, byte[] payload, int offset, int count)
			: this(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(payload, offset, count))
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, ArraySegment<byte> payload)
			: this(new IPEndPoint(ipAddress, port), payload)
		{
		}

		public UdpReceiveMessage(string ipAddress, int port, byte[] payload)
			: this(IPAddress.Parse(ipAddress), port, payload)
		{
		}

		public UdpReceiveMessage(string ipAddress, int port, ArraySegment<byte> payload)
			: this(IPAddress.Parse(ipAddress), port, payload)
		{
		}

		public UdpReceiveMessage(IPEndPoint endPoint, ArraySegment<byte> payload)
		{
			EndPoint = endPoint;
			Payload = payload;
		}

		public UdpReceiveMessage(IPEndPoint endPoint, byte[] payload, bool @partial)
			: this(endPoint, new ArraySegment<byte>(payload), @partial)
		{
		}

		public UdpReceiveMessage(IPEndPoint endPoint, byte[] payload, int offset, int count, bool @partial)
			: this(endPoint, new ArraySegment<byte>(payload, offset, count), @partial)
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, byte[] payload, bool @partial)
			: this(new IPEndPoint(ipAddress, port), payload, @partial)
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, byte[] payload, int offset, int count, bool @partial)
			: this(new IPEndPoint(ipAddress, port), new ArraySegment<byte>(payload, offset, count), @partial)
		{
		}

		public UdpReceiveMessage(IPAddress ipAddress, int port, ArraySegment<byte> payload, bool @partial)
			: this(new IPEndPoint(ipAddress, port), payload, @partial)
		{
		}

		public UdpReceiveMessage(string ipAddress, int port, byte[] payload, bool @partial)
			: this(IPAddress.Parse(ipAddress), port, payload, @partial)
		{
		}

		public UdpReceiveMessage(string ipAddress, int port, ArraySegment<byte> payload, bool @partial)
			: this(IPAddress.Parse(ipAddress), port, payload, @partial)
		{
		}

		public UdpReceiveMessage(IPEndPoint endPoint, ArraySegment<byte> payload, bool @partial)
			: this(endPoint, payload)
		{
			Partial = @partial;
		}

		public bool Partial { get; set; }
	}
}

