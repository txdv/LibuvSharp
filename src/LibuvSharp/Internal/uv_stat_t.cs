using System;

namespace LibuvSharp
{
	struct uv_stat_t {
		public ulong st_dev;
		public ulong st_mode;
		public ulong st_nlink;
		public ulong st_uid;
		public ulong st_gid;
		public ulong st_rdev;
		public ulong st_ino;
		public ulong st_size;
		public ulong st_blksize;
		public ulong st_blocks;
		public ulong st_flags;
		public ulong st_gen;
		public uv_timespec_t st_atim;
		public uv_timespec_t st_mtim;
		public uv_timespec_t st_ctim;
		public uv_timespec_t st_birthtim;
	}
}

