using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVDirectoryAsync
	{
		public static Task Create(string name)
		{
			return Create(Loop.Default, name);
		}
		public static Task Create(Loop loop, string name)
		{
			return Create(Loop.Default, name);
		}
		public static Task Create(string name, int mode)
		{
			return Create(Loop.Default, name, mode);
		}
		public static Task Create(Loop loop, string name, int mode)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				UVDirectory.Create(loop, name, mode, (e) => {
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

		public static Task Delete(string path)
		{
			return Delete(Loop.Default, path);
		}
		public static Task Delete(Loop loop, string path)
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

		public static Task Rename(string path, string newPath)
		{
			return Rename(Loop.Default, path, newPath);
		}
		public static Task Rename(Loop loop, string path, string newPath)
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

		public static Task<string[]> Read(string path)
		{
			return Read(Loop.Default, path);
		}
		public static Task<string[]> Read(this Loop loop, string path)
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

