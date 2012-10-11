using System;

namespace LibuvSharp
{
	public interface IListener<TStream> where TStream : IUVStream
	{
		int DefaultBacklog { get; set; }
		void Listen(int backlog);
		void Listen();
		event Action IncommingStream;
		TStream AcceptStream();
	}
}

