using System;

namespace LibuvSharp
{
	public interface IMessageReceiver
	{
		void Receive(ArraySegment<byte> buffer, Action<Exception, UdpReceiveMessage> message);
	}
}

