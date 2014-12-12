using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class UVFileAsync
	{
		public static Task Chmod(string path, int mode)
		{
			return Chmod(Loop.Constructor, path, mode);
		}
		public static Task Chmod(Loop loop, string path, int mode)
		{
			return HelperFunctions.Wrap(loop, path, mode, UVFile.Chmod);
		}

		public static Task Chown(string path, int uid, int gid)
		{
			return Chown(Loop.Constructor, path, uid, gid);
		}
		public static Task Chown(Loop loop, string path, int uid, int gid)
		{
			return HelperFunctions.Wrap(loop, path, uid, gid, UVFile.Chown);
		}

		public static Task Link(string path, string newPath)
		{
			return Link(Loop.Constructor, path, newPath);
		}
		public static Task Link(Loop loop, string path, string newPath)
		{
			return HelperFunctions.Wrap(loop, path, newPath, UVFile.Link);
		}

		public static Task Unlink(string path)
		{
			return Unlink(Loop.Constructor, path);
		}
		public static Task Unlink(Loop loop, string path)
		{
			return HelperFunctions.Wrap(loop, path, UVFile.Unlink);
		}

		public static Task<UVFile> Open(string path, UVFileAccess access)
		{
			return Open(Loop.Constructor, path, access);
		}
		public static Task<UVFile> Open(Loop loop, string path, UVFileAccess access)
		{
			return HelperFunctions.Wrap<Loop, string, UVFileAccess, UVFile>(loop, path, access, UVFile.Open);
		}

		public static Task Symlink(Loop loop, string path, string newPath)
		{
			return HelperFunctions.Wrap(loop, path, newPath, UVFile.Symlink);
		}
		public static Task Symlink(string path, string newPath)
		{
			return Symlink(Loop.Constructor, path, newPath);
		}

		public static Task<string> Readlink(Loop loop, string path)
		{
			return HelperFunctions.Wrap<Loop, string, string>(loop, path, UVFile.Readlink);
		}
		public static Task<string> Readlink(string path)
		{
			return Readlink(Loop.Constructor, path);
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
			return HelperFunctions.Wrap(loop, file.Sync);
		}
		public static Task SyncAsync(this UVFile file)
		{
			return file.SyncAsync(file.Loop);
		}

		public static Task DataSyncAsync(this UVFile file, Loop loop)
		{
			return HelperFunctions.Wrap(loop, file.DataSync);
		}
		public static Task DataSyncAsync(this UVFile file)
		{
			return file.DataSyncAsync(file.Loop);
		}

		public static Task TruncateSync(this UVFile file, Loop loop, int offset)
		{
			return HelperFunctions.Wrap(loop, offset, file.Truncate);
		}
		public static Task TruncateSync(this UVFile file, int offset)
		{
			return file.TruncateSync(file.Loop, offset);
		}

		public static Task ChmodAsync(this UVFile file, Loop loop, int mode)
		{
			return HelperFunctions.Wrap(loop, mode, file.Chmod);
		}
		public static Task ChmodAsync(this UVFile file, int mode)
		{
			return file.ChmodAsync(file.Loop, mode);
		}

		public static Task ChownAsync(this UVFile file, Loop loop, int uid, int gid)
		{
			return HelperFunctions.Wrap(loop, uid, gid, file.Chown);
		}
		public static Task ChownAsync(this UVFile file, int uid, int gid)
		{
			return file.ChownAsync(file.Loop, uid, gid);
		}

		public static Task<UVFileStat> Stat(Loop loop, string path)
		{
			return HelperFunctions.Wrap<Loop, string, UVFileStat>(loop, path, UVFile.Stat);
		}
		public static Task<UVFileStat> Stat(string path)
		{
			return Stat(Loop.Constructor, path);
		}

		public static Task<UVFileStat> StatAsync(this UVFile file, Loop loop)
		{
			return HelperFunctions.Wrap<Loop, UVFileStat>(loop, file.Stat);
		}
		public static Task<UVFileStat> StatAsync(this UVFile file)
		{
			return file.StatAsync(file.Loop);
		}
	}
}

