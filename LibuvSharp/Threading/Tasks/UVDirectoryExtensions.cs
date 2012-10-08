using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVDirectoryExtensions
	{
		public static Task UVCreateDirectoryAsync(this Loop loop, string name)
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

		public static Task UVDeleteDirectoryAsync(this Loop loop, string path)
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

		public static Task UVRenameDirectoryAsync(this Loop loop, string path, string newPath)
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

		public static Task<string[]> UVReadDirectoryAsync(this Loop loop, string path)
		{
			var tcs = new TaskCompletionSource<string[]>();
			try {
				UVDirectory.Read(loop, path, (e, dirs) => {
					if (e == null) {
						tcs.SetResult(dirs);
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

