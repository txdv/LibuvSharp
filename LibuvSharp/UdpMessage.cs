using System;
using System.Net;

namespace LibuvSharp
{
	public struct UdpMessage
	{
		readonly IPEndPoint endPoint;
		readonly ArraySegment<byte> data;
		readonly bool @partial;

		public UdpMessage(IPEndPoint endPoint, ArraySegment<byte> data, bool @partial)
		{
			this.endPoint = endPoint;
			this.data = data;
			this.@partial = @partial;
		}

		public IPEndPoint IPEndPoint {
			get {
				return endPoint;
			}
		}

		public ArraySegment<byte> Data {
			get {
				return data;
			}
		}

		public bool Partial {
			get {
				return @partial;
			}
		}
	}
}

