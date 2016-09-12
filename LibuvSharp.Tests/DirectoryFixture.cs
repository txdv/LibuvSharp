using System;
using System.IO;
using System.Linq;
using Xunit;

namespace LibuvSharp.Tests
{
	public class DirectoryFixture : Fixture
	{
		[Fact]
		public void CreateDirectory()
		{
			if (Directory.Exists(Default.Directory)) {
				Directory.Delete(Default.Directory, true);
			}

			UVDirectory.Create(Default.Directory, 511, (e) => {
				Assert.Null(e);
				Assert.True(Directory.Exists(Default.Directory));
				Directory.Delete(Default.Directory);
			});

			Loop.Current.Run();
		}

		public void DeleteDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Delete(Default.Directory, (e) => {
				Assert.Null(e);
			});

			Loop.Current.Run();
		}

		public void RenameDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Rename(Default.Directory, Default.SecondDirectory, (e) => {
				Assert.Null(e);
				Assert.False(Directory.Exists(Default.Directory));
				Assert.True(Directory.Exists(Default.SecondDirectory));
				Directory.Delete(Default.SecondDirectory);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void ReadEmptyDirectory()
		{
			if (!Directory.Exists(Default.Directory)) {
				Directory.CreateDirectory(Default.Directory);
			}

			UVDirectory.Read(Default.Directory, (e, list) => {
				Assert.Null(e);
				Assert.NotNull(list);
				Assert.Equal(list.Length, 0);
				Directory.Delete(Default.Directory);
			});

			Loop.Current.Run();
		}

		[Fact]
		public void ReadNotEmptyDirectory()
		{
			Directory.CreateDirectory(Default.Directory);
			Directory.CreateDirectory(Path.Combine(Default.Directory, "dir"));
			File.CreateText(Path.Combine(Default.Directory, "file")).Close();


			UVDirectory.Read(Default.Directory, (e, list) => {
				Assert.Null(e);
				Assert.NotNull(list);
				Assert.Equal(list.Length, 2);

				Assert.True(list.Select(entity => entity.Name).Contains("dir"));
				Assert.True(list.Select(entity => entity.Name).Contains("file"));

				Directory.Delete(Default.Directory, true);
			});

			Loop.Current.Run();
		}
	}
}

