using System;
using System.Text;
using System.IO;
using LibuvSharp;

namespace Test
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				string file = Path.Combine(Directory.GetCurrentDirectory(), "Test.exe");
				var stdout = new Pipe() { Writeable = true };
				var stream = Process.Spawn(new ProcessOptions() {
					Arguments = new string[] { file },
					File = file,
					Streams = new UVStream[] { null, stdout },
				});
				Console.Write(await stdout.ReadStringAsync());
			});
		}
	}
}
