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
			await loop.CreateDirectoryAsync("testing");
			await loop.RenameDirectoryAsync("testing", "testing123");
			await loop.DeleteDirectoryAsync("testing123");
		});
	}

	public static void Main(string[] args)
	{
		Execute(Loop.Default);
	}
}
