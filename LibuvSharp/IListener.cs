using System;

namespace LibuvSharp
{
	public interface IListener<TStream> where TStream : IUVStream
	{
		int DefaultBacklog { get; set; }
		void Listen(int backlog, Action callback);
		void Listen(Action callback);
		TStream AcceptStream();
	}
}

