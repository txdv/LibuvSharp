using System;
using System.Text;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class ProcessFixture
	{
		[Test]
		public void Base()
		{
			Assert.NotNull(Process.Title);
			Assert.IsNotNullOrEmpty(Process.ExecutablePath);
		}

		Process ProcessSpawn(string command, Action<int> callback)
		{
			var args = command.Split(new char[] { ' ' });
			return Process.Spawn(new ProcessOptions() {
				File = args[0],
				Arguments = args
			}, (process) => {
				callback(process.ExitCode);
			});
		}

		Process ProcessSpawn(string command, Action<string> callback)
		{
			var stdout = new Pipe() { Writeable = true };

			var streams = new UVStream[] {
				null,
				stdout,
			};

			string text = null;

			var args = command.Split(new char[] { ' ' });
			var p = Process.Spawn(new ProcessOptions() {
				File = args[0],
				Arguments = args,
				Streams = streams
			}, (process) => {
				callback(text);
			});

			stdout.Read(Encoding.ASCII, (t) => text = t);
			stdout.Resume();
			return p;
		}

		[Test]
		public void ProcessSpawn()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				string result = null;
				ProcessSpawn("/usr/bin/which which", (res) => result = res);
				Loop.Default.Run();
				Assert.AreEqual(result, "/usr/bin/which\n");

				int? exitCode = null;
				ProcessSpawn("/usr/bin/which which", (res) => exitCode = res);
				Loop.Default.Run();
				Assert.AreEqual(exitCode.Value, 0);
			}
		}
	}
}

