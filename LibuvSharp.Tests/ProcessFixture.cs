using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp.Threading.Tasks;
using Xunit;

namespace LibuvSharp.Tests
{
	public class ProcessFixture
	{
		[Fact]
		public void Base()
		{
			Assert.NotNull(Process.Title);
			var path = Process.ExecutablePath;
			Assert.NotNull(path);
			Assert.NotEqual(path, string.Empty);
		}

		[Fact]
		public void ProcessSpawn()
		{
			string file = Default.TestExecutable;
			using (var stdout = new Pipe() { Writeable = true })
			using (var p = Process.Spawn(new ProcessOptions() {
				File = file,
				Arguments = new string[] { file },
				Streams = new UVStream [] { null, stdout }
			}, (process) => {
				stdout.Dispose();
			})) {

				//for some reason this ain't working
				//stdout.Read(Encoding.ASCII, (result) => Assert.Equal("Hello World!", result));
				stdout.Resume();

				Loop.Default.Run();

				Assert.Equal(0, p.ExitCode);
			}
			Loop.Default.Run();
		}

		[Fact]
		public void ProcessSpawnAsync()
		{
			Loop.Default.Run(async () => {
				string file = Default.TestExecutable;
				using (var stdout = new Pipe() { Writeable = true })
				using (Process.Spawn(new ProcessOptions() {
					Arguments = new string[] { file },
					File = file,
					Streams = new UVStream[] { null, stdout },
				})) {
					var segment = (await stdout.ReadStructAsync()).Value;
					var output = Encoding.Default.GetString(segment.Array, segment.Offset, segment.Count);
					Assert.Equal("Hello World!", output.TrimEnd('\r', '\n'));
				}
			});
		}
	}
}

