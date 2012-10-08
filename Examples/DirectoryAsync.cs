using System;
using System.IO;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	public static void Execute(Loop loop)
	{
		loop.Run(async delegate {
			try {
				await loop.Async(() => Directory.CreateDirectory("testing"));
				await loop.Async(() => Directory.Move("testing", "testing123"));
				var dirs = await loop.FAsync(() => Directory.GetFiles("./"));
				Console.WriteLine("{0} files.", dirs.Length);
				Console.WriteLine();
				foreach (var dir in dirs) {
					Console.WriteLine(dir);
				}
				await loop.Async(() => Directory.Delete("testing123"));
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		});
	}

	public static void Main(string[] args)
	{
		Execute(Loop.Default);
	}
}
