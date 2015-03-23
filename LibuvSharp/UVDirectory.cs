using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LibuvSharp
{
	public class UVDirectory
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_mkdir(IntPtr loop, IntPtr req, string path, int mode, NativeMethods.uv_fs_cb callback);

		public static void Create(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_mkdir(loop.NativeHandle, fsr.Handle, path, mode, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}
		public static void Create(Loop loop, string path, int mode)
		{
			Create(loop, path, mode, null);
		}
		public static void Create(Loop loop, string path, Action<Exception> callback)
		{
			Create(loop, path, 511, callback);
		}
		public static void Create(Loop loop, string path)
		{
			Create(loop, path, 511);
		}
		public static void Create(string path, int mode, Action<Exception> callback)
		{
			Create(Loop.Constructor, path, mode, callback);
		}
		public static void Create(string path, Action<Exception> callback)
		{
			Create(Loop.Constructor, path, 511, callback);
		}
		public static void Create(string path, int mode)
		{
			Create(path, mode, null);
		}
		public static void Create(string path)
		{
			Create(path, 511);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_rmdir(IntPtr loop, IntPtr req, string path, NativeMethods.uv_fs_cb callback);

		public static void Delete(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_rmdir(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}
		public static void Delete(Loop loop, string path)
		{
			Delete(loop, path, null);
		}
		public static void Delete(string path, Action<Exception> callback)
		{
			Delete(Loop.Constructor, path, callback);
		}
		public static void Delete(string path)
		{
			Delete(path, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_rename(IntPtr loop, IntPtr req, string path, string newPath, NativeMethods.uv_fs_cb callback);

		public static void Rename(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_rename(loop.NativeHandle, fsr.Handle, path, newPath, fsr.End);
			Ensure.Success(r);
		}
		public static void Rename(Loop loop, string path, string newPath)
		{
			Rename(loop, path, newPath, null);
		}
		public static void Rename(string path, string newPath, Action<Exception> callback)
		{
			Rename(Loop.Constructor, path, newPath, callback);
		}
		public static void Rename(string path, string newPath)
		{
			Rename(path, newPath, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_scandir_next(IntPtr req, out uv_dirent_t ent);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_scandir(IntPtr loop, IntPtr req, string path, int flags, NativeMethods.uv_fs_cb callback);

		public static void Read(Loop loop, string path, Action<Exception, UVDirectoryEntity[]> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (ex != null) {
					callback(ex, null);
					return;
				}

				var list = new List<UVDirectoryEntity>();
				uv_dirent_t entity;
				while (UVException.Map(uv_fs_scandir_next(fsr.Handle, out entity)) != UVErrorCode.EOF) {
					list.Add(new UVDirectoryEntity(entity));
				}

				Ensure.Success(ex, callback, list.ToArray());
			};
			int r = uv_fs_scandir(loop.NativeHandle, fsr.Handle, path, 0, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}

		public static void Read(string path, Action<Exception, UVDirectoryEntity[]> callback)
		{
			Read(Loop.Constructor, path, callback);
		}
	}
}

