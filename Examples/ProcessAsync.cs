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
				using (var stdout = new Pipe() { Writeable = true })
				using (var process = Process.Spawn(new ProcessOptions() {
					File = file,
					Arguments = new string[] { file },
					Streams = new UVStream[] { null, stdout },
				})) {
					Console.WriteLine(await stdout.ReadStringAsync());
				}
			});
		}
	}
}
