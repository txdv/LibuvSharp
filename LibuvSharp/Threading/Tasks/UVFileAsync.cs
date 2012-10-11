using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVFileAsync
	{
		static Task Wrap(Loop loop, Action<Loop, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(loop, (e) => {
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
		static Task Wrap<T1>(Loop loop, T1 obj1, Action<Loop, T1, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(loop, obj1, (e) => {
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
		static Task Wrap<T1, T2>(Loop loop, T1 obj1, T2 obj2, Action<Loop, T1, T2, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(loop, obj1, obj2, (e) => {
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
		static Task Wrap<T1, T2, T3>(Loop loop, T1 obj1, T2 obj2, T3 obj3, Action<Loop, T1, T2, T3, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(loop, obj1, obj2, obj3, (e) => {
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

		static Task<R> FWrap<R>(Loop loop, Action<Loop, Action<Exception, R>> action)
		{
			var tcs = new TaskCompletionSource<R>();
			try {
				action(loop, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		static Task<R> FWrap<R, T1>(Loop loop, T1 obj1, Action<Loop, T1, Action<Exception, R>> action)
		{
			var tcs = new TaskCompletionSource<R>();
			try {
				action(loop, obj1, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		static Task<R> FWrap<R, T1, T2>(Loop loop, T1 obj1, T2 obj2, Action<Loop, T1, T2, Action<Exception, R>> action)
		{
			var tcs = new TaskCompletionSource<R>();
			try {
				action(loop, obj1, obj2, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		static Task<R> FWrap<R, T1, T2, T3>(Loop loop, T1 obj1, T2 obj2, T3 obj3, Action<Loop, T1, T2, T3, Action<Exception, R>> action)
		{
			var tcs = new TaskCompletionSource<R>();
			try {
				action(loop, obj1, obj2, obj3, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				});
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}

		public static Task Chmod(string path, int mode)
		{
			return Chmod(Loop.Default, path, mode);
		}
		public static Task Chmod(Loop loop, string path, int mode)
		{
			return Wrap(loop, path, mode, UVFile.Chmod);
		}

		public static Task Chown(string path, int uid, int gid)
		{
			return Chown(Loop.Default, path, uid, gid);
		}
		public static Task Chown(Loop loop, string path, int uid, int gid)
		{
			return Wrap(loop, path, uid, gid, UVFile.Chown);
		}

		public static Task Link(string path, string newPath)
		{
			return Link(Loop.Default, path, newPath);
		}
		public static Task Link(Loop loop, string path, string newPath)
		{
			return Wrap(loop, path, newPath, UVFile.Link);
		}

		public static Task Unlink(string path)
		{
			return Unlink(Loop.Default, path);
		}
		public static Task Unlink(Loop loop, string path)
		{
			return Wrap(loop, path, UVFile.Unlink);
		}

		public static Task<UVFile> Open(string path, UVFileAccess access)
		{
			return Open(Loop.Default, path, access);
		}
		public static Task<UVFile> Open(Loop loop, string path, UVFileAccess access)
		{
			return FWrap<UVFile, string, UVFileAccess>(loop, path, access, UVFile.Open);
		}

		public static Task Symlink(Loop loop, string path, string newPath)
		{
			return Wrap(loop, path, newPath, UVFile.Symlink);
		}
		public static Task Symlink(string path, string newPath)
		{
			return Symlink(Loop.Default, path, newPath);
		}

		public static Task<string> Readlink(Loop loop, string path)
		{
			return FWrap<string, string>(loop, path, UVFile.Readlink);
		}
		public static Task<string> Readlink(string path)
		{
			return Readlink(Loop.Default, path);
		}

		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data, int index, int count, int offset)
		{
			var tcs = new TaskCompletionSource<int>();
			try {
				file.Read(loop, data, index, count, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				}, offset);
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data, int index, int count)
		{
			return file.ReadAsync(loop, data, index, count, -1);
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data, int index)
		{
			return file.ReadAsync(loop, data, index, data.Length - index);
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data)
		{
			return file.ReadAsync(loop, data, 0);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data, int index, int count, int offset)
		{
			return file.ReadAsync(file.Loop, data, index, count, offset);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data, int index, int count)
		{
			return file.ReadAsync(file.Loop, data, index, count);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data, int index)
		{
			return file.ReadAsync(file.Loop, data, index);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data)
		{
			return file.ReadAsync(file.Loop, data);
		}

		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data, int index, int count, int offset)
		{
			var tcs = new TaskCompletionSource<int>();
			try {
				file.Write(loop, data, index, count, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				}, offset);
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data, int index, int count)
		{
			return file.WriteAsync(loop, data, index, count, -1);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data, int index)
		{
			return file.WriteAsync(loop, data, index, data.Length - index);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data)
		{
			return file.WriteAsync(loop, data, 0);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data, int index, int count, int offset)
		{
			return file.WriteAsync(file.Loop, data, index, count, offset);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data, int index, int count)
		{
			return file.WriteAsync(file.Loop, data, index, count);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data, int index)
		{
			return file.WriteAsync(file.Loop, data, index);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data)
		{
			return file.WriteAsync(file.Loop, data);
		}

		public static Task<int> WriteAsync(this UVFile file, Loop loop, Encoding encoding, string text, int offset)
		{
			var tcs = new TaskCompletionSource<int>();
			try {
				file.Write(loop, encoding, text, (e, result) => {
					if (e == null) {
						tcs.SetResult(result);
					} else {
						tcs.SetException(e);
					}
				}, offset);
			} catch (Exception e) {
				tcs.SetException(e);
			}
			return tcs.Task;
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, Encoding encoding, string text)
		{
			return file.WriteAsync(loop, encoding, text, -1);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, string text, int offset)
		{
			return file.WriteAsync(loop, Encoding.Default, text, offset);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, string text)
		{
			return file.WriteAsync(loop, text, -1);
		}
		public static Task<int> WriteAsync(this UVFile file, Encoding encoding, string text, int offset)
		{
			return file.WriteAsync(file.Loop, encoding, text, offset);
		}
		public static Task<int> WriteAsync(this UVFile file, Encoding encoding, string text)
		{
			return file.WriteAsync(file.Loop, encoding, text);
		}
		public static Task<int> WriteAsync(this UVFile file, string text, int offset)
		{
			return file.WriteAsync(file.Loop, text, offset);
		}
		public static Task<int> WriteAsync(this UVFile file, string text)
		{
			return file.WriteAsync(file.Loop, text);
		}

		public static Task SyncAsync(this UVFile file, Loop loop)
		{
			return Wrap(loop, file.Sync);
		}
		public static Task SyncAsync(this UVFile file)
		{
			return file.SyncAsync(file.Loop);
		}

		public static Task DataSyncAsync(this UVFile file, Loop loop)
		{
			return Wrap(loop, file.DataSync);
		}
		public static Task DataSyncAsync(this UVFile file)
		{
			return file.DataSyncAsync(file.Loop);
		}

		public static Task TruncateSync(this UVFile file, Loop loop, int offset)
		{
			return Wrap(loop, offset, file.Truncate);
		}
		public static Task TruncateSync(this UVFile file, int offset)
		{
			return file.TruncateSync(file.Loop, offset);
		}

		public static Task ChmodAsync(this UVFile file, Loop loop, int mode)
		{
			return Wrap(loop, mode, file.Chmod);
		}
		public static Task ChmodAsync(this UVFile file, int mode)
		{
			return file.ChmodAsync(file.Loop, mode);
		}

		public static Task ChownAsync(this UVFile file, Loop loop, int uid, int gid)
		{
			return Wrap(loop, uid, gid, file.Chown);
		}
		public static Task ChownAsync(this UVFile file, int uid, int gid)
		{
			return file.ChownAsync(file.Loop, uid, gid);
		}

		public static Task<UVFileStat> Stat(Loop loop, string path)
		{
			return FWrap<UVFileStat, string>(loop, path, UVFile.Stat);
		}
		public static Task<UVFileStat> Stat(string path)
		{
			return Stat(Loop.Default, path);
		}

		public static Task<UVFileStat> StatAsync(this UVFile file, Loop loop)
		{
			return FWrap<UVFileStat>(loop, file.Stat);
		}
		public static Task<UVFileStat> StatAsync(this UVFile file)
		{
			return file.StatAsync(file.Loop);
		}
	}
}

