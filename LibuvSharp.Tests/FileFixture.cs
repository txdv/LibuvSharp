using System;
using System.IO;
using NUnit.Framework;

namespace LibuvSharp.Tests
{

	[TestFixture]
	public class FileFixture
	{
		[TestCase]
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

