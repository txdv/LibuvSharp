using System;
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
	}
}

