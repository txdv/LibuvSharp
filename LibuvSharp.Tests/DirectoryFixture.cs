using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class DirectoryFixture
	{
		[TestCase]
		public void CreateDirectory()
		{
			if (Directory.Exists(Default.Directory)) {
				Directory.Delete(Default.Directory, true);
			}

			UVDirectory.Create(Default.Directory, 511, (e) => {
				Assert.IsNull(e);
				Assert.IsTrue(Directory.Exists(Default.Directory));
				Directory.Delete(Default.Directory);
			});

			Loop.Default.Run();
		}

		public void DeleteDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Delete(Default.Directory, (e) => {
				Assert.IsNull(e);
			});

			Loop.Default.Run();
		}

		public void RenameDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Rename(Default.Directory, Default.SecondDirectory, (e) => {
				Assert.IsNull(e);
				Assert.IsFalse(Directory.Exists(Default.Directory));
				Assert.IsTrue(Directory.Exists(Default.SecondDirectory));
				Directory.Delete(Default.SecondDirectory);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void ReadEmptyDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Read(Default.Directory, (e, list) => {
				Assert.IsNull(e);
				Assert.IsNotNull(list);
				Assert.AreEqual(list.Length, 0);
				Directory.Delete(Default.Directory);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void ReadNotEmptyDirectory()
		{
			Directory.CreateDirectory(Default.Directory);
			Directory.CreateDirectory(Path.Combine(Default.Directory, "dir"));
			File.CreateText(Path.Combine(Default.Directory, "file")).Close();


			UVDirectory.Read(Default.Directory, (e, list) => {
				Assert.IsNull(e);
				Assert.IsNotNull(list);
				Assert.AreEqual(list.Length, 2);
				Assert.IsTrue(list.Contains("dir"));
				Assert.IsTrue(list.Contains("file"));

				Directory.Delete(Default.Directory, true);
			});

			Loop.Default.Run();
		}
	}
}

