using System;

namespace LibuvSharp
{
	public class UVFileStat
	{
		public ulong DeviceID { get; set; }
		public uint Node { get; set; }
		public uint Mode { get; set; }
		public uint LinkCount { get; set; }
		public uint UID { get; set; }
		public uint GID { get; set; }
		public ulong RDeviceID { get; set; }
		public int Size { get; set; }
		public int BlockSize { get; set; }
		public int BlockCount { get; set; }
		public int AccessTime { get; set; }
		public int ModificationTime { get; set; }
		public int StatusChangeTime { get; set; }
	}
}

