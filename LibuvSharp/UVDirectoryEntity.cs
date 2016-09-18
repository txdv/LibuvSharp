using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class UVDirectoryEntity
	{
		internal unsafe UVDirectoryEntity(uv_dirent_t entity)
		{
			Name = PlatformApis.NewString(entity.name);
			Type = entity.type;
		}

		public string Name { get; set; }
		public UVDirectoryEntityType Type { get; set; }
	}
}

