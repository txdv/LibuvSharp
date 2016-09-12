using System;
using System.IO;
using Xunit;

namespace LibuvSharp.Tests
{

	public class FileFixture : Fixture
	{
		public void CreateNotexistingFile()
		{
			if (File.Exists(Default.File)) {
				File.Delete(Default.File);
			}

			UVFile.Open(Default.File, UVFileAccess.Write, (e, file) => {
				Assert.Null(e);
				Assert.NotNull(file);
			});

			Loop.Current.Run();
		}
	}
}

