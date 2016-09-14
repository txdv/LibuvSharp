using System;

namespace LibuvSharp
{
	public class UVFileStat
	{
		internal UVFileStat(uv_stat_t stat)
		{
			Device = stat.st_dev;
			Mode = stat.st_mode;
			Link = stat.st_nlink;

			UID = stat.st_uid;
			GID = stat.st_gid;

			rdev = stat.st_rdev;
			ino = stat.st_ino;
			Size = stat.st_size;
			BlockSize = stat.st_blksize;
			Blocks = stat.st_blocks;
			Flags = stat.st_flags;
			gen = stat.st_gen;

			Access = stat.st_atim.ToDateTime();
			Modify = stat.st_mtim.ToDateTime();
			Change = stat.st_ctim.ToDateTime();
			Birth = stat.st_birthtim.ToDateTime();

		}

		public UVFileStat()
		{
		}

		public ulong Device { get; set; }
		public ulong Mode { get; set; }
		public ulong Link { get; set; }
		public ulong UID { get; set; }
		public ulong GID { get; set; }
		public ulong rdev { get; set; }
		public ulong ino { get; set; }
		public ulong Size { get; set; }
		public ulong BlockSize { get; set; }
		public ulong Blocks { get; set; }
		public ulong Flags { get; set; }
		public ulong gen { get; set; }
		public DateTime Access { get; set; }
		public DateTime Modify { get; set; }
		public DateTime Change { get; set; }
		public DateTime Birth { get; set; }
	}
}

