using System;
using System.Threading;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;

namespace LibuvSharp
{
	public partial class Loop
	{
		TaskFactory taskfactory = null;
		public TaskFactory TaskFactory {
			get {
				if (taskfactory == null) {
					taskfactory = new TaskFactory(Scheduler);
				}
				return taskfactory;
			}
		}

		public TaskScheduler Scheduler {
			get {
				return LoopTaskScheduler.Instance;
			}
		}

		public static Loop Current
		{
			get {
				var current = SynchronizationContext.Current;
				if (current is LoopSynchronizationContext) {
					return (current as LoopSynchronizationContext).Loop;
				}

				// TODO: find appropriate exception
				throw new Exception();
			}
		}
	}
}

