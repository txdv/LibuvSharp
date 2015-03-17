using System;
using System.IO;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	/*
	 * This example shows how to use blocking calls with the
	 * LibuvSharp event loop. As you see, you can just call Task.Run
	 * inside of a Run(Task<Task>), inside it will use a "special"
	 * LoopSynchronizationContext to synchronize the the Tasks into
	 * the event loop.
	 */
	public static void Main(string[] args)
	{
		Loop.Default.Run(async () => {
			await Task.Run(() => Directory.CreateDirectory("testing"));
			await Task.Run(() => Directory.Move("testing", "testing123"));
			var dirs = await Task.Run(() => Directory.GetFiles("./"));
			Console.WriteLine("{0} files:", dirs.Length);
			foreach (var dir in dirs) {
				Console.WriteLine(dir);
			}
			await Task.Run(() => Directory.Delete("testing123"));
		});
	}
}
