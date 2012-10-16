using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct lin_stat
	{
		#pragma warning disable 169
		public ulong dev;
		uint offset1;
		public uint ino;
		public uint mode;
		public uint nlink;
		public uint uid;
		public uint gid;
		public ulong rdev;
		uint offset2;
		public int size;
		uint offset3;
		public int blksize;
		public int blkcnt;
		uint offset4;
		public int atime;
		public int mtime;
		public int ctime;
		#pragma warning restore 169

		public override string ToString ()
		{
			return string.Format(
				"dev={0}\n" +
				"ino={1}\n" +
				"mode={2}\n" +
				"nlink={3}\n" +
				"uid={4}\n" +
				"gid={5}\n" +
				"rdev={6}\n" +
				"size={7}\n" +
				"blksize={8}\n" +
				"blkcnt={9}\n" +
				"atime={10}\n" +
				"mtime={11}\n" +
				"ctime={12}",
				dev, ino, mode, nlink, uid, gid, rdev, size, blksize, blkcnt, atime, mtime, ctime);
		}

		public void Assign(UVFileStat stat)
		{
			stat.DeviceID = dev;
			stat.Node = ino;
			stat.Mode = mode;
			stat.LinkCount = nlink;
			stat.UID = uid;
			stat.GID = gid;
			stat.RDeviceID = rdev;
			stat.Size = size;
			stat.BlockSize = blksize;
			stat.BlockCount = blkcnt;
			stat.AccessTime = atime;
			stat.ModificationTime = mtime;
			stat.StatusChangeTime = ctime;
		}

		unsafe public static UVFileStat Convert(IntPtr ptr)
		{
			UVFileStat stat = new UVFileStat();
			((lin_stat *)ptr)->Assign(stat);
			return stat;
		}
	}
}

