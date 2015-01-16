using System;

namespace LibuvSharp
{
	public interface IMessageSender<TEndPoint, TMessage>
	{
		void Send(TEndPoint endPoint, TMessage message, Action<Exception> callback);
	}
}

