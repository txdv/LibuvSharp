using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	public static void Main(string[] args)
	{
		Loop.Default.Run(async () => {
			var t = new UVTimer();
			var now = DateTime.Now;
			await t.StartAsync(TimeSpan.FromSeconds(1));
			Console.WriteLine(DateTime.Now - now);
		});
	}
}
