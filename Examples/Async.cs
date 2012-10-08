using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

class LoopSyncContext : SynchronizationContext
{
	public override void Post(SendOrPostCallback d, object state)
	{
		d(state);
	}
}

static class LoopExtensions
{
	public static void Run(this Loop loop, Func<Task> asyncMethod)
	{
		var prevCtx = SynchronizationContext.Current;
		try {
			SynchronizationContext.SetSynchronizationContext(new LoopSyncContext());
			var task = asyncMethod();
			task.ContinueWith(delegate {
				loop.Unref();
			}, loop.Scheduler);
			loop.Ref();
			loop.Run();
		} finally {
			SynchronizationContext.SetSynchronizationContext(prevCtx);
		}
	}
}

static class AsyncExtensions
{
	public static async Task<string> ReadStringAsync(this IUVStream stream)
	{
		return await ReadStringAsync(stream, Encoding.Default);
	}

	public static async Task<string> ReadStringAsync(this IUVStream stream, Encoding encoding)
	{
		if (encoding == null) {
			throw new ArgumentException("encoding");
		}
		var buffer = await stream.ReadAsync();
		if (buffer == null) {
			return null;
		}
		return encoding.GetString(buffer.Buffer, buffer.Start, buffer.Length);
	}

	public static Task QueueUserWorkItemAsync(this Loop loop, Action work)
	{
		var tcs = new TaskCompletionSource<object>();
		try {
			loop.QueueUserWorkItem(work, () => {
				tcs.SetResult(null);
			});
		} catch (Exception e) {
			tcs.SetException(e);
		}
		return tcs.Task;
	}
}
