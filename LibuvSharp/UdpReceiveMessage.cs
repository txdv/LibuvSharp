using System;
using System.Net;

namespace LibuvSharp
{
	public class UdpReceiveMessage : UdpMessage
	{
		internal UdpReceiveMessage()
			: base()
		{
		}

		internal UdpReceiveMessage(IPEndPoint endPoint, ArraySegment<byte> data, bool @partial)
			: base(endPoint, data)
		{
			Partial = @partial;
		}

		public bool Partial { get; set; }
	}
}

