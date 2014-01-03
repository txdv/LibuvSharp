using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public class LoopSynchronizationContext : SynchronizationContext
	{
		public Loop Loop { get; private set; }

		public LoopSynchronizationContext(Loop loop)
		{
			Loop = loop;
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			d(state);
		}
	}
}

