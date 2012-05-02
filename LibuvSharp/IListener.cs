using System;

namespace LibuvSharp
{
	public interface IListener
	{
		int DefaultBacklog { get; set; }
		void Listen(int backlog, Action<Stream> callback);
		void Listen(Action<Stream> callback);
	}
}

