using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibuvSharp;

namespace LibuvSharp.Threading.Tasks
{
	public class ListenerAsync<TListener, TStream> : IDisposable where TListener : IListener<TStream> where TStream : IUVStream
	{
		public TListener Listener { get; private set; }
		public int WaitingConnections { get; private set; }

		Queue<TaskCompletionSource<TStream>> queue = new Queue<TaskCompletionSource<TStream>>();

		public ListenerAsync(TListener listener)
		{
			Listener = listener;
			listener.IncommingStream += HandleIncommingStream;
		}

		bool disposed = false;
		public void Dispose()
		{
			if (!disposed) {
				Listener.IncommingStream -= HandleIncommingStream;
				disposed = true;
			}
		}

		void HandleIncommingStream()
		{
			if (queue.Count > 0) {
				queue.Dequeue().SetResult(Listener.AcceptStream());;
			} else {
				WaitingConnections++;
			}
		}

		public Task<TStream> AcceptStreamAsync()
		{
			var tcs = new TaskCompletionSource<TStream>();
			if (WaitingConnections > 0) {
				WaitingConnections--;
				tcs.SetResult(Listener.AcceptStream());
			} else {
				queue.Enqueue(tcs);
			}
			return tcs.Task;
		}
	}
}

