using System;

namespace LibuvSharp
{
	public interface IMessageReceiver<TMessage>
	{
		event Action<TMessage> Message;
	}
}

