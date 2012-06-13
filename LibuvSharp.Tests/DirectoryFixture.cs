using System;
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
			if (System.IO.Directory.Exists(Default.Directory)) {
				System.IO.Directory.Delete(Default.Directory);
			}

			Directory.Create(Default.Directory, 511, (e) => {
				Assert.IsNull(e);
				Assert.IsTrue(System.IO.Directory.Exists(Default.Directory));
				System.IO.Directory.Delete(Default.Directory);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void DeleteDirectory()
		{
			if (!System.IO.Directory.Exists(Default.Directory)) {
				System.IO.Directory.CreateDirectory(Default.Directory);
			}

			Directory.Delete(Default.Directory, (e) => {
				Assert.IsNull(e);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void RenameDirectory()
		{
			if (!System.IO.Directory.Exists(Default.Directory)) {
				System.IO.Directory.CreateDirectory(Default.Directory);
			}

			Directory.Rename(Default.Directory, Default.SecondDirectory, (e) => {
				Assert.IsNull(e);
				Assert.IsFalse(System.IO.Directory.Exists(Default.Directory));
				Assert.IsTrue(System.IO.Directory.Exists(Default.SecondDirectory));
				System.IO.Directory.Delete(Default.SecondDirectory);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void ReadEmptyDirectory()
		{
			if (!System.IO.Directory.Exists(Default.Directory)) {
				System.IO.Directory.CreateDirectory(Default.Directory);
			}

			Directory.Read(Default.Directory, (e, list) => {
				Assert.IsNull(e);
				Assert.IsNotNull(list);
				Assert.AreEqual(list.Length, 0);
				System.IO.Directory.Delete(Default.Directory);
			});

			Loop.Default.Run();
		}

		[TestCase]
		public void ReadNotEmptyDirectory()
		{
			System.IO.Directory.CreateDirectory(Default.Directory);
			System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Default.Directory, "dir"));
			System.IO.File.CreateText(System.IO.Path.Combine(Default.Directory, "file")).Close();


			Directory.Read(Default.Directory, (e, list) => {
				Assert.IsNull(e);
				Assert.IsNotNull(list);
				Assert.AreEqual(list.Length, 2);
				Assert.IsTrue(list.Contains("dir"));
				Assert.IsTrue(list.Contains("file"));

				System.IO.Directory.Delete(Default.Directory, true);
			});

			Loop.Default.Run();
		}
	}
}

