using System;
using System.Net;

namespace LibuvSharp
{
	public interface IDatagram<TEndPoint>
	{
		void Send(TEndPoint endPoint, ArraySegment<byte> data, Action<Exception> callback);
		void Receive(ArraySegment<byte> buffer, Action<Exception, UdpReceiveMessage> message);
	}
}

