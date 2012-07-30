using System;

namespace LibuvSharp
{
	public interface IListener
	{
		int DefaultBacklog { get; set; }
		void Listen(int backlog, Action<UVStream> callback);
		void Listen(Action<UVStream> callback);
	}
}

