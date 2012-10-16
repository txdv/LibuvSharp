using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace LibuvSharp
{
	public class UVDirectory
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void uv_fs_cb(IntPtr IntPtr);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_mkdir(IntPtr loop, IntPtr req, string path, int mode, uv_fs_cb callback);

		public static void Create(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_mkdir(loop.NativeHandle, fsr.Handle, path, mode, FileSystemRequest.StaticEnd);
			Ensure.Success(r, loop);
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
			Create(Loop.Default, path, mode, callback);
		}
		public static void Create(string path, Action<Exception> callback)
		{
			Create(Loop.Default, path, 511, callback);
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
		private static extern int uv_fs_rmdir(IntPtr loop, IntPtr req, string path, uv_fs_cb callback);

		public static void Delete(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_rmdir(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.StaticEnd);
			Ensure.Success(r, loop);
		}
		public static void Delete(Loop loop, string path)
		{
			Delete(loop, path, null);
		}
		public static void Delete(string path, Action<Exception> callback)
		{
			Delete(Loop.Default, path, callback);
		}
		public static void Delete(string path)
		{
			Delete(path, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_rename(IntPtr loop, IntPtr req, string path, string newPath, uv_fs_cb callback);

		public static void Rename(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_rename(loop.NativeHandle, fsr.Handle, path, newPath, fsr.End);
			Ensure.Success(r, loop);
		}
		public static void Rename(Loop loop, string path, string newPath)
		{
			Rename(loop, path, newPath, null);
		}
		public static void Rename(string path, string newPath, Action<Exception> callback)
		{
			Rename(Loop.Default, path, newPath, callback);
		}
		public static void Rename(string path, string newPath)
		{
			Rename(path, newPath, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_readdir(IntPtr loop, IntPtr req, string path, int flags, uv_fs_cb callback);

		unsafe public static void Read(Loop loop, string path, Action<Exception, string[]> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (ex != null) {
					callback(ex, null);
					return;
				}

				int length = (int)fsr.Result;
				string[] res = new string[length];
				sbyte *ptr = (sbyte *)fsr.Pointer;
				for (int i = 0; i < length; i++) {
					res[i] = new string(ptr);
					ptr += res[i].Length + 1;
				}
				callback(ex, res);
			};
			uv_fs_readdir(loop.NativeHandle, fsr.Handle, path, 0, FileSystemRequest.StaticEnd);
		}
		public static void Read(string path, Action<Exception, string[]> callback)
		{
			Read(Loop.Default, path, callback);
		}
	}
}

