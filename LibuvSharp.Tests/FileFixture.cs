using System;
using System.IO;
using NUnit.Framework;

namespace LibuvSharp.Tests
{

	public class FileFixture
	{
		public void CreateNotexistingFile()
		{
			if (File.Exists(Default.File)) {
				File.Delete(Default.File);
			}

			UVFile.Open(Default.File, FileAccess.Write, (e, file) => {
				Assert.IsNull(e);
				Assert.IsNotNull(file);
			});

			Loop.Default.Run();
		}
	}
}

