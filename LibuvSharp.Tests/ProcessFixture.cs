using System;
using System.Text;
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

		Process ProcessSpawnTest(string command, Action<long> callback)
		{
			var args = command.Split(new char[] { ' ' });
			return Process.Spawn(new ProcessOptions() {
				File = args[0],
				Arguments = args
			}, (process) => {
				callback(process.ExitCode);
			});
		}

		Process ProcessSpawnTest(string command, Action<string> callback)
		{
			var stdout = new Pipe() { Writeable = true };

			var streams = new UVStream[] {
				null,
				stdout,
			};

			var args = command.Split(new char[] { ' ' });
			var p = Process.Spawn(new ProcessOptions() {
				File = args[0],
				Arguments = args,
				Streams = streams
			});

			stdout.Read(Encoding.ASCII, callback);
			stdout.Resume();
			return p;
		}

		[Fact]
		public void ProcessSpawn()
		{
			string command = "csc.exe";
			string resultString = "Microsoft ";
			
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				command = "/usr/bin/which which";
				resultString = "/usr/bin/which\n";
			}

			string result = null;
			ProcessSpawnTest(command, (res) => result = res);
			Loop.Default.Run();
			Assert.Equal(result, resultString);

			long? exitCode = null;
			ProcessSpawnTest(command, (res) => exitCode = res);
			Loop.Default.Run();
			Assert.Equal(exitCode.Value, 0);
		}
	}
}

