using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	static ConsoleColor GetColor(UVDirectoryEntityType type)
	{
		switch (type) {
		case UVDirectoryEntityType.Directory:
			return ConsoleColor.Yellow;
		case UVDirectoryEntityType.Link:
			return ConsoleColor.Blue;
		case UVDirectoryEntityType.File:
			return ConsoleColor.White;
		}
		throw new Exception("If you encounter a new file type, please add an appropriate color!");
	}

	public static void Main(string[] args)
	{
		Loop.Default.Run(async delegate {
			try {
				await UVDirectoryAsync.Create("testing");
				await UVDirectoryAsync.Rename("testing", "testing123");
				var files = await UVDirectoryAsync.Read("./");
				Console.WriteLine("{0} files totally", files.Length);
				Console.WriteLine();

				// first order by name, then by type
				foreach (var file in files.OrderBy((file) => file.Name).OrderBy((file) => file.Type)) {
					Console.ForegroundColor = GetColor(file.Type);
					Console.WriteLine(file.Name);
				}

				await UVDirectoryAsync.Delete("testing123");
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		});
	}
}
