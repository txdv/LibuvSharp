using System;
using System.Threading.Tasks;

namespace LibuvSharp
{
	public static class PipeExtensions
	{
		public static Task ConnectAsync(this Pipe pipe, string name)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				pipe.Connect(name, (e) => {
					if (e == null) {
						tcs.SetResult(null);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
	}
}

