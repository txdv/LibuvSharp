using System;
using System.Threading.Tasks;

namespace LibuvSharp
{
	public static class UVFileStreamExtensions
	{
		public static Task OpenAsync(this UVFileStream filestream, string path, UVFileAccess access)
		{
			return HelperFunctions.Wrap(path, access, filestream.Open);
		}

		public static Task OpenReadAsync(this UVFileStream fileStream, string path)
		{
			return HelperFunctions.Wrap(path, fileStream.OpenRead);
		}

		public static Task OpenWriteAsync(this UVFileStream fileStream, string path)
		{
			return HelperFunctions.Wrap(path, fileStream.OpenWrite);
		}
	}
}

