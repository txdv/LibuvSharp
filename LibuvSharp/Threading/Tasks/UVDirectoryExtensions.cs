using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVDirectoryExtensions
	{
		public static Task CreateDirectoryAsync(this Loop loop, string name)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				UVDirectory.Create(loop, name, (e) => {
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

		public static Task DeleteDirectoryAsync(this Loop loop, string path)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				UVDirectory.Delete(loop, path, (e) => {
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

		public static Task RenameDirectoryAsync(this Loop loop, string path, string newPath)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				UVDirectory.Rename(loop, path, newPath, (e) => {
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

