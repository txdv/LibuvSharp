using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	class LoopTaskScheduler : System.Threading.Tasks.TaskScheduler
	{
		static LoopTaskScheduler()
		{
			Instance = new LoopTaskScheduler();
		}

		public static LoopTaskScheduler Instance { get; private set; }

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return new Task[] { };
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			TryExecuteTask(task);
			return true;
		}

		protected override void QueueTask(Task task)
		{
			TryExecuteTask(task);
		}
	}
}

