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
		private static extern int uv_fs_open(IntPtr loop, IntPtr req, string path, int flags, int mode, NativeMethods.uv_fs_cb callback);

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
			int r = uv_fs_open(loop.NativeHandle, fsr.Handle, path, (int)access, 0, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}
		public static void Open(string path, UVFileAccess access, Action<Exception, UVFile> callback)
		{
			Open(Loop.Constructor, path, access, callback);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_close(IntPtr loop, IntPtr req, int fd, NativeMethods.uv_fs_cb callback);

		public void Close(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_close(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_read(IntPtr loop, IntPtr req, int fd, uv_buf_t[] buf, int nbufs, long offset, NativeMethods.uv_fs_cb callback);

		public void Read(Loop loop, int offset, ArraySegment<byte> segment, Action<Exception, int> callback)
		{
			var datagchandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				Ensure.Success(ex, callback, fsr.Result.ToInt32());
				datagchandle.Free();
			};
			var ptr = new IntPtr(datagchandle.AddrOfPinnedObject().ToInt64() + segment.Offset);
			var buf = new uv_buf_t[] { new uv_buf_t(ptr, segment.Count) };
			int r = uv_fs_read(loop.NativeHandle, fsr.Handle, FileDescriptor, buf, 1, offset, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}

		public void Read(Loop loop, int offset, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(loop, offset, new ArraySegment<byte>(data, index, count), callback);
		}
		public void Read(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(loop, -1, data, index, count, callback);
		}
		public void Read(Loop loop, int offset, byte[] data, Action<Exception, int> callback)
		{
			Ensure.ArgumentNotNull(data, "data");
			Read(loop, offset, data, 0, data.Length, callback);
		}
		public void Read(Loop loop, byte[] data, Action<Exception, int> callback)
		{
			Read(loop, -1, data, callback);
		}
		public void Read(Loop loop, int offset, byte[] data, int index, int count)
		{
			Read(loop, offset, data, index, count, null);
		}
		public void Read(Loop loop, byte[] data, int index, int count)
		{
			Read(loop, -1, data, index, count);
		}
		public void Read(Loop loop, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Read(loop, data, 0, data.Length);
		}

		public void Read(int offset, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(this.Loop, offset, data, index, count, callback);
		}
		public void Read(byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Read(this.Loop, data, index, count, callback);
		}
		public void Read(int offset, byte[] data, Action<Exception, int> callback)
		{
			Read(this.Loop, offset, data, callback);
		}
		public void Read(byte[] data, Action<Exception, int> callback)
		{
			Read(this.Loop, data, callback);
		}
		public void Read(int offset, byte[] data, int index, int count)
		{
			Read(this.Loop, offset, data, index, count);
		}
		public void Read(byte[] data, int index, int count)
		{
			Read(this.Loop, data, index, count);
		}
		public void Read(byte[] data)
		{
			Read(this.Loop, data);
		}

		public void Read(Loop loop, ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Read(loop, -1, data, callback);
		}
		public void Read(Loop loop, int offset, ArraySegment<byte> data)
		{
			Read(loop, offset, data, null);
		}
		public void Read(Loop loop, ArraySegment<byte> data)
		{
			Read(loop, data, null);
		}

		public void Read(int offset, ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Read(Loop, offset, data, callback);
		}
		public void Read(ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Read(-1, data, callback);
		}
		public void Read(int offset, ArraySegment<byte> data)
		{
			Read(offset, data, null);
		}
		public void Read(ArraySegment<byte> data)
		{
			Read(data, null);
		}

		public int Read(Loop loop, int offset, Encoding encoding, string text, Action<Exception, int> callback)
		{
			var bytes = encoding.GetBytes(text);
			Read(loop, offset, bytes, callback);
			return bytes.Length;
		}
		public int Read(Loop loop, Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Read(loop, -1, encoding, text, callback);
		}
		public int Read(Loop loop, int offset, Encoding encoding, string text)
		{
			return Read(loop, offset, encoding, text, null);
		}
		public int Read(Loop loop, Encoding encoding, string text)
		{
			return Read(loop, -1, encoding, text);
		}
		public int Read(Loop loop, int offset, string text, Action<Exception, int> callback)
		{
			return Read(loop, offset, Encoding ?? Encoding.Default, text, callback);
		}
		public int Read(Loop loop, string text, Action<Exception, int> callback)
		{
			return Read(loop, -1, text, callback);
		}
		public int Read(Loop loop, int offset, string text)
		{
			return Read(loop, offset, text, null);
		}
		public int Read(Loop loop, string text)
		{
			return Read(loop, -1, text);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_write(IntPtr loop, IntPtr req, int fd, uv_buf_t[] bufs, int nbufs, long offset, NativeMethods.uv_fs_cb fs_cb);

		public void Write(Loop loop, int offset, ArraySegment<byte> segment, Action<Exception, int> callback)
		{
			var datagchandle = GCHandle.Alloc(segment.Array, GCHandleType.Pinned);
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				Ensure.Success(ex, callback, fsr.Result.ToInt32());
				datagchandle.Free();
			};
			var ptr = new IntPtr(datagchandle.AddrOfPinnedObject().ToInt64() + segment.Offset);
			var buf = new uv_buf_t[] { new uv_buf_t(ptr, segment.Count) };
			int r = uv_fs_write(loop.NativeHandle, fsr.Handle, FileDescriptor, buf, segment.Count, offset, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}

		public void Write(Loop loop, int offset, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(loop, offset, new ArraySegment<byte>(data, index, count), callback);
		}
		public void Write(Loop loop, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(loop, -1, data, index, count, callback);
		}
		public void Write(Loop loop, int offset, byte[] data, Action<Exception, int> callback)
		{
			Write(loop, offset, data, 0, data.Length, callback);
		}
		public void Write(Loop loop, byte[] data, Action<Exception, int> callback)
		{
			Write(loop, -1, data, callback);
		}
		public void Write(Loop loop, int offset, byte[] data, int index, int count)
		{
			Write(loop, offset, data, index, count, null);
		}
		public void Write(Loop loop, byte[] data, int index, int count)
		{
			Write(loop, -1, data, index, count);
		}
		public void Write(Loop loop, byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Write(loop, data, 0, data.Length);
		}

		public void Write(int offset, byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(this.Loop, offset, data, index, count, callback);
		}
		public void Write(byte[] data, int index, int count, Action<Exception, int> callback)
		{
			Write(-1, data, index, count, callback);
		}
		public void Write(int offset, byte[] data, Action<Exception, int> callback)
		{
			Write(this.Loop, offset, data, callback);
		}
		public void Write(byte[] data, Action<Exception, int> callback)
		{
			Write(this.Loop, data, callback);
		}
		public void Write(int offset, byte[] data, int index, int count)
		{
			Write(offset, data, index, count, null);
		}
		public void Write(byte[] data, int index, int count)
		{
			Write(-1, data, index, count);
		}
		public void Write(byte[] data)
		{
			Ensure.ArgumentNotNull(data, "data");
			Write(data, 0, data.Length);
		}

		public void Write(Loop loop, ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Write(loop, -1, data, callback);
		}
		public void Write(int offset, Loop loop, ArraySegment<byte> data)
		{
			Write(loop, offset, data, null);
		}
		public void Write(Loop loop, ArraySegment<byte> data)
		{
			Write(loop, data, null);
		}

		public void Write(int offset, ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Write(Loop, offset, data, callback);
		}
		public void Write(ArraySegment<byte> data, Action<Exception, int> callback)
		{
			Write(-1, data, callback);
		}
		public void Write(int offset, ArraySegment<byte> data)
		{
			Write(offset, data, null);
		}
		public void Write(ArraySegment<byte> data)
		{
			Write(data, null);
		}

		public int Write(Loop loop, int offset, Encoding encoding, string text, Action<Exception, int> callback)
		{
			var bytes = encoding.GetBytes(text);
			Write(loop, offset, bytes, callback);
			return bytes.Length;
		}
		public int Write(Loop loop, Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Write(loop, -1, encoding, text, callback);
		}
		public int Write(Loop loop, int offset, Encoding encoding, string text)
		{
			return Write(loop, offset, encoding, text, null);
		}
		public int Write(Loop loop, Encoding encoding, string text)
		{
			return Write(loop, -1, encoding, text);
		}
		public int Write(Loop loop, int offset, string text, Action<Exception, int> callback)
		{
			return Write(loop, offset, Encoding ?? Encoding.Default, text, callback);
		}
		public int Write(Loop loop, string text, Action<Exception, int> callback)
		{
			return Write(loop, -1, text, callback);
		}
		public int Write(Loop loop, int offset, string text)
		{
			return Write(loop, offset, text, null);
		}
		public int Write(Loop loop, string text)
		{
			return Write(loop, -1, text);
		}

		public int Write(int offset, Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, offset, encoding, text, callback);
		}
		public int Write(Encoding encoding, string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, encoding, text, callback);
		}
		public int Write(int offset, Encoding encoding, string text)
		{
			return Write(this.Loop, offset, encoding, text);
		}
		public int Write(Encoding encoding, string text)
		{
			return Write(this.Loop, encoding, text);
		}
		public int Write(int offset, string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, offset, text, callback);
		}
		public int Write(string text, Action<Exception, int> callback)
		{
			return Write(this.Loop, text, callback);
		}
		public int Write(int offset, string text)
		{
			return Write(this.Loop, offset, text);
		}
		public int Write(string text)
		{
			return Write(this.Loop, text);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_stat(IntPtr loop, IntPtr req, string path, NativeMethods.uv_fs_cb callback);

		public static void Stat(Loop loop, string path, Action<Exception, UVFileStat> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (callback != null) {
					Ensure.Success(ex, callback, new UVFileStat(fsr.stat));
				}
			};
			int r = uv_fs_stat(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}

		public static void Stat(string path, Action<Exception, UVFileStat> callback)
		{
			Stat(Loop.Constructor, path, callback);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fstat(IntPtr loop, IntPtr req, int fd, NativeMethods.uv_fs_cb callback);

		public void Stat(Loop loop, Action<Exception, UVFileStat> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = (ex) => {
				if (callback != null) {
					Ensure.Success(ex, callback, new UVFileStat(fsr.stat));
				}
			};
			int r = uv_fs_fstat(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int uv_fs_fsync(IntPtr loop, IntPtr req, int fd, NativeMethods.uv_fs_cb callback);

		public void Sync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fsync(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_fdatasync(IntPtr loop, IntPtr req, int fd, NativeMethods.uv_fs_cb callback);

		public void DataSync(Loop loop, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fdatasync(loop.NativeHandle, fsr.Handle, FileDescriptor, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_ftruncate(IntPtr loop, IntPtr req, int file, long offset, NativeMethods.uv_fs_cb callback);

		public void Truncate(Loop loop, int offset, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_ftruncate(loop.NativeHandle, fsr.Handle, FileDescriptor, offset, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_fchmod(IntPtr loop, IntPtr req, int fd, int mode, NativeMethods.uv_fs_cb callback);

		public void Chmod(Loop loop, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fchmod(loop.NativeHandle, fsr.Handle, FileDescriptor, mode, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_chmod(IntPtr loop, IntPtr req, string path, int mode, NativeMethods.uv_fs_cb callback);

		public static void Chmod(Loop loop, string path, int mode, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_chmod(loop.NativeHandle, fsr.Handle, path, mode, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_chown(IntPtr loop, IntPtr req, string path, int uid, int gid, NativeMethods.uv_fs_cb callback);

		public static void Chown(Loop loop, string path, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_chown(loop.NativeHandle, fsr.Handle, path, uid, gid, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_fchown(IntPtr loop, IntPtr req, int fd, int uid, int gid, NativeMethods.uv_fs_cb callback);

		public void Chown(Loop loop, int uid, int gid, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_fchown(loop.NativeHandle, fsr.Handle, FileDescriptor, uid, gid, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_unlink(IntPtr loop, IntPtr req, string path, NativeMethods.uv_fs_cb callback);

		public static void Unlink(Loop loop, string path, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_unlink(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_link(IntPtr loop, IntPtr req, string path, string newPath, NativeMethods.uv_fs_cb callback);

		public static void Link(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_link(loop.NativeHandle, fsr.Handle, path, newPath, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_symlink(IntPtr loop, IntPtr req, string path, string newPath, int flags, NativeMethods.uv_fs_cb callback);

		public static void Symlink(Loop loop, string path, string newPath, Action<Exception> callback)
		{
			var fsr = new FileSystemRequest();
			fsr.Callback = callback;
			int r = uv_fs_symlink(loop.NativeHandle, fsr.Handle, path, newPath, 0, FileSystemRequest.CallbackDelegate);
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
		private static extern int uv_fs_readlink(IntPtr loop, IntPtr req, string path, NativeMethods.uv_fs_cb callback);

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
			int r = uv_fs_readlink(loop.NativeHandle, fsr.Handle, path, FileSystemRequest.CallbackDelegate);
			Ensure.Success(r);
		}
		public static void Readlink(string path, Action<Exception, string> callback)
		{
			Readlink(Loop.Constructor, path, callback);
		}
	}
}

