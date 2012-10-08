using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	public static void Main(string[] args)
	{
		Loop.Default.Run(async delegate {
			try {
				await UVDirectoryAsync.Create("testing");
				await UVDirectoryAsync.Rename("testing", "testing123");
				var dirs = await UVDirectoryAsync.Read("./");
				Console.WriteLine("{0} files totally", dirs.Length);
				Console.WriteLine();
				foreach (var dir in dirs) {
					Console.WriteLine(dir);
				}
				await UVDirectoryAsync.Delete("testing123");
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		});
	}
}
