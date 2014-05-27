using System;
using System.Text;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class ProcessFixture
	{
		[TestCase]
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

		[TestCase]
		public void ProcessSpawn()
		{
			string command = "csc.exe";
			string resultString = "Microsoft ";
			
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				command = "/usr/bin/which which";
				resultString = "/usr/bin/which\n";
			}

			string result = null;
			ProcessSpawn(command, (res) => result = res);
			Loop.Default.Run();
			Assert.AreEqual(result, resultString);

			int? exitCode = null;
			ProcessSpawn(command, (res) => exitCode = res);
			Loop.Default.Run();
			Assert.AreEqual(exitCode.Value, 0);
		}
	}
}

