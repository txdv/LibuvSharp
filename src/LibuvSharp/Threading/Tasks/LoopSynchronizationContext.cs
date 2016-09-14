using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public class LoopSynchronizationContext : SynchronizationContext
	{
		public Loop Loop { get; private set; }

		public Thread Thread { get; private set; }

		public int PendingOperations { get; private set; }

		public LoopSynchronizationContext(Loop loop)
			: this(loop, Thread.CurrentThread)
		{
		}

		public LoopSynchronizationContext(Loop loop, Thread thread)
		{
			Loop = loop;
			Thread = thread;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			if (Thread == Thread.CurrentThread) {
				d(state);
			} else {
				Loop.Sync(() => d(state));
			}
		}

		public override void OperationStarted()
		{
			PendingOperations++;
			Loop.Ref();
		}

		public override void OperationCompleted()
		{
			Loop.Unref();
			PendingOperations--;
		}
	}
}

