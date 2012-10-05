using System;
using System.Collections.Generic;
using System.Text;
using LibuvSharp;

namespace Test
{
	class MainClass
	{
		static Process ProcessSpawn(string command, Action<string> callback)
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

		public static void Main(string[] args)
		{
			string[] bins = new string[] { "which", "bin", "gmcs", "clang" , "notexisting" };

			for (int i = 0; i < bins.Length; i++) {
				var that = i;
				string file = bins[that];
				ProcessSpawn(string.Format("/usr/bin/which {0}",  file), (res) => {
					if (res != null) {
						Console.WriteLine(res.TrimEnd(new char [] { '\r', '\n' }));
					}
				});
			}
			Loop.Default.Run();
			Console.WriteLine(new object[] { } is ICollection<object>);
		}
	}
}
