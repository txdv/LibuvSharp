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

		void ProcessSpawn(string command, Action<int, string> callback)
		{
			var args = command.Split(new char[] { ' ' });
			Pipe stdout = new Pipe();
			stdout.Open((IntPtr)1);
			var p = Process.Spawn(new ProcessOptions() {
				File = args[0],
				Arguments = args,
				Stdout = stdout
			}, (process) => {
				stdout.Shutdown();
				stdout.Close();
			});
			stdout.Read(Encoding.ASCII, (text) => callback(p.ExitCode, text));
		}

		[Test]
		public void ProcessSpawn()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				int result = -1;
				string which = null;
				ProcessSpawn("/usr/bin/which which", (code, res) => {
					result = code;
					which = res;
				});
				Loop.Default.Run();
				Assert.AreEqual(result, 0);
				Assert.AreEqual(which, "/usr/bin/which");
			}
		}
	}
}

