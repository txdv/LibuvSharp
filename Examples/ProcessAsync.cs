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
			/*
			 * This works on unix only if .exe files are somehow associated
			 * with /usr/bin/mono. A good way to test it if they are is to
			 * set them to executables (chmod +x Test.exe) and then just
			 * try to run from command line: ./Test.exe
			 * If they execute, this example should work as well
			 */
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
