using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	public static void Execute(Loop loop)
	{
		loop.Run(async delegate {
			try {
				await loop.UVCreateDirectoryAsync("testing");
				await loop.UVRenameDirectoryAsync("testing", "testing123");
				var dirs = await loop.UVReadDirectoryAsync("./");
				Console.WriteLine("{0} files totally", dirs.Length);
				Console.WriteLine();
				foreach (var dir in dirs) {
					Console.WriteLine(dir);
				}
				await loop.UVDeleteDirectoryAsync("testing123");
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
