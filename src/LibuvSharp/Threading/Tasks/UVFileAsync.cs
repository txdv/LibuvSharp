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

		#region ReadAsync

		public static Task<int> ReadAsync(this UVFile file, Loop loop, int offset, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<Loop, int, ArraySegment<byte>, int>(loop, offset, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, int offset, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<int, ArraySegment<byte>, int>(offset, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<Loop, ArraySegment<byte>, int>(loop, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<ArraySegment<byte>, int>(data, file.Read);
		}

		public static Task<int> ReadAsync(this UVFile file, Loop loop, int offset, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<Loop, int, byte[], int, int, int>(loop, offset, data, index, count, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, int offset, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<int, byte[], int, int, int>(offset, data, index, count, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<Loop, byte[], int, int, int>(loop, data, index, count, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data, int index, int count)

		{
			return HelperFunctions.Wrap<byte[], int, int, int>(data, index, count, file.Read);
		}

		public static Task<int> ReadAsync(this UVFile file, Loop loop, int offset, byte[] data)
		{
			return HelperFunctions.Wrap<Loop, int, byte[], int>(loop, offset, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, int offset, byte[] data)
		{
			return HelperFunctions.Wrap<int, byte[], int>(offset, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, Loop loop, byte[] data)
		{
			return HelperFunctions.Wrap<Loop, byte[], int>(loop, data, file.Read);
		}
		public static Task<int> ReadAsync(this UVFile file, byte[] data)
		{
			return HelperFunctions.Wrap<byte[], int>(data, file.Read);
		}

		#endregion

		#region WriteAsync

		public static Task<int> WriteAsync(this UVFile file, Loop loop, int offset, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<Loop, int, ArraySegment<byte>, int>(loop, offset, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, int offset, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<int, ArraySegment<byte>, int>(offset, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<Loop, ArraySegment<byte>, int>(loop, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, ArraySegment<byte> data)
		{
			return HelperFunctions.Wrap<ArraySegment<byte>, int>(data, file.Write);
		}

		public static Task<int> WriteAsync(this UVFile file, Loop loop, int offset, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<Loop, int, byte[], int, int, int>(loop, offset, data, index, count, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, int offset, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<int, byte[], int, int, int>(offset, data, index, count, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data, int index, int count)
		{
			return HelperFunctions.Wrap<Loop, byte[], int, int, int>(loop, data, index, count, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data, int index, int count)

		{
			return HelperFunctions.Wrap<byte[], int, int, int>(data, index, count, file.Write);
		}

		public static Task<int> WriteAsync(this UVFile file, Loop loop, int offset, byte[] data)
		{
			return HelperFunctions.Wrap<Loop, int, byte[], int>(loop, offset, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, int offset, byte[] data)
		{
			return HelperFunctions.Wrap<int, byte[], int>(offset, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, Loop loop, byte[] data)
		{
			return HelperFunctions.Wrap<Loop, byte[], int>(loop, data, file.Write);
		}
		public static Task<int> WriteAsync(this UVFile file, byte[] data)
		{
			return HelperFunctions.Wrap<byte[], int>(data, file.Write);
		}

		#endregion

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

