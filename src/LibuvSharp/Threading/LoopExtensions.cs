using System;
using System.Threading;

namespace LibuvSharp.Threading
{
	public static class LoopExtensions
	{
		public static void QueueUserWorkItem(this Loop loop, Action work, Action after)
		{
			loop.Ref();
			ThreadPool.QueueUserWorkItem((_) => {
				if (work != null) {
					work();
				}
				loop.Sync(() => {
					loop.Unref();
					if (after != null) {
						after();
					}
				});
			});
		}

		public static void QueueUserWorkItem<T>(this Loop loop, T state, Action<T> work, Action after)
		{
			loop.Ref();
			ThreadPool.QueueUserWorkItem((o) => {
				if (work != null) {
					work((T)o);
				}
				loop.Sync(() => {
					loop.Unref();
					if (after != null) {
						after();
					}
				});
			}, (object)state);
		}
	}
}

