using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

class MainClass
{
	public static void Main(string[] args)
	{
		Loop.Default.Run(async () => {
			var t = new UVTimer();
			var stopwatch = Stopwatch.StartNew();
			await t.StartAsync(TimeSpan.FromSeconds(1));
			stopwatch.Stop();
			Console.WriteLine(stopwatch.Elapsed);
		});
	}
}
