using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public enum UVFileAccess
	{
		Read = 0,
		Write = 1,
		ReadWrite = 3,
	}

	// TODO:
	// 1. uv_fs_utime uv_fs_futime

	public class UVFile
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void uv_fs_cb(IntPtr IntPtr);

		public UVFile(int fd)
			: this(Loop.Constructor, fd)
		{
		}

		public UVFile(Loop loop, int fd)
		{
			Loop = loop;
			FileDescriptor = fd;
		}

		public Loop Loop { get; protected set; }
		public int FileDescriptor { get; protected set; }
		public Encoding Encoding { get; set; }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_open(LoopSafeHandle loop, IntPtr req, string path, int flags, int mode, uv_fs_cb callback);

		public static void Open(Loop loop, string path, UVFileAccess access, Action<Exception, UVFile> callback)
		{
			var fsr = new FileSystemRequest(path);
			fsr.Callback = (ex) => {
				UVFile file = null;
				if (fsr.Result != IntPtr.Zero) {
					file = new UVFile(loop, fsr.Result.ToInt32());
				}
				Ensure.Success(ex, callback, file);
			};
			int r = uv_fs_open(loop.NativeHandle, fsr.Handle, path, (int)access, 0, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Open(string path, UVFileAccess access, Action<Exception, UVFile> callback)
		{
			Open(Loop.Constructor, path, access, callback);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_close(LoopSafeHandle loop, IntPtr req, int fd, uv_fs_cb callback);

		public void Close(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_close(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Close(Loop loop)
		{
			Close(loop, null);
		}
		public void Close(Action<Exception> callback)
		{
			Close(Loop, callback);
		}
		public void Close()
		{
			Close(Loop);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_read(LoopSafeHandle loop, IntPtr req, int fd, IntPtr buf, IntPtr length, long offset, uv_fs_cb callback);

		public void Read(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback, int offset)
		{
			GCHandle datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				Ensure.Success(ex, callback, fsr.Result.ToInt32());
				datagchandle.Free();
			};
			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + index);
			int r = uv_fs_read(loop.NativeHandle, fsr.Handle, FileDescriptor, ptr, (IntPtr)count, offset, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Read(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(loop, data, index, count, callback, -1);
		}
		public void Read(Loop loop, byte[] data, int index, Action<Exception, int> callback, int offset)
		{
			Read(loop, data, index, data.Length - index, callback, offset);
		}
		public void Read(Loop loop, byte[] data, int index, Action<Exception, int> callback)
		{
			Read(loop, data, index, callback, -1);
		}
		public void Read(Loop loop, byte[] data, Action<Exception, int> callback, int offset)
		{
			Read(loop, data, 0, callback, offset);
		}
		public void Read(Loop loop, byte[] data, Action<Exception, int> callback)
		{
			Read(loop, data, callback, -1);
		}
		public void Read(Loop loop, byte[] data, int index, int count, int offset)
		{
			Read(loop, data, index, count, null, offset);
		}
		public void Read(Loop loop, byte[] data, int index, int count)
		{
			Read(loop, data, index, count, -1);
		}
		public void Read(Loop loop, byte[] data, int index)
		{
			Read(loop, data, index, data.Length - index);
		}
		public void Read(Loop loop, byte[] data)
		{
			Read(loop, data, 0);
		}

		public void Read(byte[] data, int index, int count, Action<Exception, int> callback, int offset)
		{
			Read(this.Loop, data, index, count, callback, offset);
		}
		public void Read(byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(this.Loop, data, index, count, callback);
		}
		public void Read(byte[] data, int index, Action<Exception, int> callback, int offset)
		{
			Read(this.Loop, data, index, callback, offset);
		}
		public void Read(byte[] data, int index, Action<Exception, int> callback)
		{
			Read(this.Loop, data, index, callback);
		}
		public void Read(byte[] data, Action<Exception, int> callback, int offset)
		{
			Read(this.Loop, data, callback, offset);
		}
		public void Read(byte[] data, Action<Exception, int> callback)
		{
			Read(this.Loop, data, callback);
		}
		public void Read(byte[] data, int index, int count, int offset)
		{
			Read(this.Loop, data, index, count, offset);
		}
		public void Read(byte[] data, int index, int count)
		{
			Read(this.Loop, data, index, count);
		}
		public void Read(byte[] data, int index)
		{
			Read(this.Loop, data, index);
		}
		public void Read(byte[] data)
		{
			Read(this.Loop, data);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_write(LoopSafeHandle loop, IntPtr req, int fd, IntPtr buf, IntPtr length, long offset, uv_fs_cb fs_cb);

		public void Write(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback, int offset)
		{
			var datagchandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				Ensure.Success(ex, callback, fsr.Result.ToInt32());
				datagchandle.Free();
			};
			var ptr = (IntPtr)(datagchandle.AddrOfPinnedObject().ToInt64() + index);
			int r = uv_fs_write(loop.NativeHandle, fsr.Handle, FileDescriptor, ptr, (IntPtr)count, offset, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Write(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(loop, data, index, count, callback, -1);
		}
		public void Write(Loop loop, byte[] data, int index, Action<Exception, int> callback, int offset)
		{
			Write(loop, data, index, data.Length - index, callback, offset);
		}
		public void Write(Loop loop, byte[] data, int index, Action<Exception, int> callback)
		{
			Write(loop, data, index, callback, -1);
		}
		public void Write(Loop loop, byte[] data, Action<Exception, int> callback, int offset)
		{
			Write(loop, data, 0, data.Length, callback, offset);
		}
		public void Write(Loop loop, byte[] data, Action<Exception, int> callback)
		{
			Write(loop, data, callback, -1);
		}
		public void Write(Loop loop, byte[] data, int index, int count, int offset)
		{
			Write(loop, data, index, count, null, offset);
		}
		public void Write(Loop loop, byte[] data, int index, int count)
		{
			Write(loop, data, index, count, -1);
		}
		public void Write(Loop loop, byte[] data, int index)
		{
			Write(loop, data, index, data.Length - index);
		}
		public void Write(Loop loop, byte[] data)
		{
			Write(loop, data, 0);
		}

		public void Write(byte[] data, int index, int count, Action<Exception, int> callback, int offset)
		{
			Write(this.Loop, data, index, count, callback, offset);
		}
		public void Write(byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(data, index, count, callback, -1);
		}
		public void Write(byte[] data, int index, Action<Exception, int> callback, int offset)
		{
			Write(this.Loop, data, index, data.Length - index, callback, offset);
		}
		public void Write(byte[] data, int index, Action<Exception, int> callback)
		{
			Write(this.Loop, data, index, callback);
		}
		public void Write(byte[] data, Action<Exception, int> callback, int offset)
		{
			Write(this.Loop, data, callback, offset);
		}
		public void Write(byte[] data, Action<Exception, int> callback)
		{
			Write(this.Loop, data, callback);
		}
		public void Write(byte[] data, int index, int count, int offset)
		{
			Write(data, index, count, null, offset);
		}
		public void Write(byte[] data, int index, int count)
		{
			Write(data, index, count, -1);
		}
		public void Write(byte[] data, int index)
		{
			Write(data, index, data.Length - index);
		}
		public void Write(byte[] data)
		{
			Write(data, 0);
		}

		public void Write(Loop loop, ArraySegment<byte> data, Action<Exception, int> callback, int offset)
		{
			Write(loop, data.Array, data.Offset, data.Count, callback, offset);
		}
		public void Write(Loop loop, ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Write(loop, data, callback, -1);
		}
		public void Write(Loop loop, ArraySegment<byte> data, int offset)
		{
			Write(loop, data, null, offset);
		}
		public void Write(Loop loop, ArraySegment<byte> data)
		{
			Write(loop, data, null);
		}

		public void Write(ArraySegment<byte> data, Action<Exception, int> callback, int offset)
		{
			Write(Loop, data, callback, offset);
		}
		public void Write(ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Write(data, callback, -1);
		}
		public void Write(ArraySegment<byte> data, int offset)
		{
			Write(data, null, offset);
		}
		public void Write(ArraySegment<byte> data)
		{
			Write(data, null);
		}

		public int Write(Loop loop, Encoding encoding, string text, Action<Exception, int> callback, int offset)
		{
			var bytes = encoding.GetBytes(text);
			Write(loop, bytes, callback, offset);
			return bytes.Length;
		}
		public int Write(Loop loop, Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Write(loop, encoding, text, callback, -1);
		}
		public int Write(Loop loop, Encoding encoding, string text, int offset)
		{
			return Write(loop, encoding, text, null, offset);
		}
		public int Write(Loop loop, Encoding encoding, string text)
		{
			return Write(loop, encoding, text, -1);
		}
		public int Write(Loop loop, string text, Action<Exception, int> callback, int offset)
		{
			return Write(loop, Encoding ?? Encoding.Default, text, callback, offset);
		}
		public int Write(Loop loop, string text, Action<Exception, int> callback)
		{
			return Write(loop, text, callback, -1);
		}
		public int Write(Loop loop, string text, int offset)
		{
			return Write(loop, text, null, offset);
		}
		public int Write(Loop loop, string text)
		{
			return Write(loop, text, -1);
		}

		public int Write(Encoding encoding, string text, Action<Exception, int> callback, int offset)
		{
			return Write(this.Loop, encoding, text, callback, offset);
		}
		public int Write(Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, encoding, text, callback);
		}
		public int Write(Encoding encoding, string text, int offset)
		{
			return Write(this.Loop, encoding, text, offset);
		}
		public int Write(Encoding encoding, string text)
		{
			return Write(this.Loop, encoding, text);
		}
		public int Write(string text, Action<Exception, int> callback, int offset)
		{
			return Write(this.Loop, text, callback, offset);
		}
		public int Write(string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, text, callback);
		}
		public int Write(string text, int offset)
		{
			return Write(this.Loop, text, offset);
		}
		public int Write(string text)
		{
			return Write(this.Loop, text);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_stat(LoopSafeHandle loop, IntPtr req, string path, uv_fs_cb callback);

		public static void Stat(Loop loop, string path, Action<Exception, UVFileStat> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (callback != null) {
					Ensure.Success(ex, callback, new UVFileStat(fsr.stat));
				}
			};
			int r = uv_fs_stat(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fstat(LoopSafeHandle loop, IntPtr req, int fd, uv_fs_cb callback);

		public void Stat(Loop loop, Action<Exception, UVFileStat> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (callback != null) {
					Ensure.Success(ex, callback, new UVFileStat(fsr.stat));
				}
			};
			int r = uv_fs_fstat(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fsync(LoopSafeHandle loop, IntPtr req, int fd, uv_fs_cb callback);

		public void Sync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fsync(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Sync(Loop loop)
		{
			Sync(loop, null);
		}
		public void Sync(Action<Exception> callback)
		{
			Sync(Loop.Constructor, callback);
		}
		public void Sync()
		{
			Sync((Action<Exception>)null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fdatasync(LoopSafeHandle loop, IntPtr req, int fd, uv_fs_cb callback);

		public void DataSync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fdatasync(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void DataSync(Loop loop)
		{
			DataSync(loop, null);
		}
		public void DataSync(Action<Exception> callback)
		{
			DataSync(Loop.Constructor, callback);
		}
		public void DataSync()
		{
			DataSync((Action<Exception>)null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_ftruncate(LoopSafeHandle loop, IntPtr req, int fd, long offset, uv_fs_cb callback);

		public void Truncate(Loop loop, int offset, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_ftruncate(loop.NativeHandle, fsr.Handle, FileDescriptor, offset, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Truncate(Loop loop, int offset)
		{
			Truncate(loop, offset);
		}
		public void Truncate(int offset, Action<Exception> callback)
		{
			Truncate(Loop.Constructor, offset, callback);
		}
		public void Truncate(int offset)
		{
			Truncate(offset, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fchmod(LoopSafeHandle loop, IntPtr req, int fd, int mode, uv_fs_cb callback);

		public void Chmod(Loop loop, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fchmod(loop.NativeHandle, fsr.Handle, FileDescriptor, mode, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Chmod(Loop loop, int mode)
		{
			Chmod(loop, mode, null);
		}
		public void Chmod(int mode, Action<Exception> callback)
		{
			Chmod(Loop.Constructor, mode, callback);
		}
		public void Chmod(int mode)
		{
			Chmod(mode, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_chmod(LoopSafeHandle loop, IntPtr req, string path, int mode, uv_fs_cb callback);

		public static void Chmod(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_chmod(loop.NativeHandle, fsr.Handle, path, mode, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Chmod(Loop loop, string path, int mode)
		{
			Chmod(loop, path, mode, null);
		}
		public static void Chmod(string path, int mode, Action<Exception> callback)
		{
			Chmod(Loop.Constructor, path, mode, callback);
		}
		public static void Chmod(string path, int mode)
		{
			Chmod(path, mode, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_chown(LoopSafeHandle loop, IntPtr req, string path, int uid, int gid, uv_fs_cb callback);

		public static void Chown(Loop loop, string path, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_chown(loop.NativeHandle, fsr.Handle, path, uid, gid, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Chown(Loop loop, string path, int uid, int gid)
		{
			Chown(loop, path, uid, gid, null);
		}
		public static void Chown(string path, int uid, int gid, Action<Exception> callback)
		{
			Chown(Loop.Constructor, path, uid, gid, callback);
		}
		public static void Chown(string path, int uid, int gid)
		{
			Chown(path, uid, gid, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fchown(LoopSafeHandle loop, IntPtr req, int fd, int uid, int gid, uv_fs_cb callback);

		public void Chown(Loop loop, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fchown(loop.NativeHandle, fsr.Handle, FileDescriptor, uid, gid, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public void Chown(Loop loop, int uid, int gid)
		{
			Chown(loop, uid, gid, null);
		}
		public void Chown(int uid, int gid, Action<Exception> callback)
		{
			Chown(Loop.Constructor, uid, gid, callback);
		}
		public void Chown(int uid, int gid)
		{
			Chown(uid, gid, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_unlink(LoopSafeHandle loop, IntPtr req, string path, uv_fs_cb callback);

		public static void Unlink(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_unlink(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Unlink(Loop loop, string path)
		{
			Unlink(loop, path, null);
		}
		public static void Unlink(string path, Action<Exception> callback)
		{
			Unlink(Loop.Constructor, path, callback);
		}
		public static void Unlink(string path)
		{
			Unlink(path, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_link(LoopSafeHandle loop, IntPtr req, string path, string newPath, uv_fs_cb callback);

		public static void Link(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_link(loop.NativeHandle, fsr.Handle, path, newPath, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Link(Loop loop, string path, string newPath)
		{
			Link(loop, path, newPath, null);
		}
		public static void Link(string path, string newPath, Action<Exception> callback)
		{
			Link(Loop.Constructor, path, newPath, callback);
		}
		public static void Link(string path, string newPath)
		{
			Link(path, newPath, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_symlink(LoopSafeHandle loop, IntPtr req, string path, string newPath, int flags, uv_fs_cb callback);

		public static void Symlink(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_symlink(loop.NativeHandle, fsr.Handle, path, newPath, 0, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Symlink(Loop loop, string path, string newPath)
		{
			Symlink(loop, path, newPath, null);
		}

		public static void Symlink(string path, string newPath, Action<Exception> callback)
		{
			Symlink(Loop.Constructor, path, newPath, callback);
		}
		public static void Symlink(string path, string newPath)
		{
			Symlink(path, newPath, null);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_readlink(LoopSafeHandle loop, IntPtr req, string path, uv_fs_cb callback);

		public static void Readlink(Loop loop, string path, Action<Exception, string> callback)
		{
			var fsr = new FileSystemRequest(path);
			fsr.Callback = (ex) => {
				string res = null;
				if (ex == null) {
					res = Marshal.PtrToStringAuto(fsr.Pointer);
				}
				Ensure.Success(ex, callback, res);
			};
			int r = uv_fs_readlink(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.StaticEnd);
			Ensure.Success(r);
		}
		public static void Readlink(string path, Action<Exception, string> callback)
		{
			Readlink(Loop.Constructor, path, callback);
		}
	}
}

