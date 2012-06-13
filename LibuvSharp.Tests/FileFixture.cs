using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{

	[TestFixture]
	public class FileFixture
	{
		[TestCase]
		public void CreateNotexistingFile()
		{
			if (System.IO.File.Exists(Default.File)) {
				System.IO.File.Delete(Default.File);
			}

			File.Open(Default.File, FileAccess.Write, (e, file) => {
				Assert.IsNull(e);
				Assert.IsNotNull(file);
			});

			Loop.Default.Run();
		}
	}
}

