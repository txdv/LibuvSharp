using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Diagnostics;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

public class MainClass
{
	public static async Task<byte[]> Compute(string file)
	{
		var stream = new UVFileStream();
		await stream.OpenReadAsync(file);
		return await Compute(stream);
	}

	public static async Task<byte[]> Compute(Uri uri)
	{
		var client = new HttpClient();
		var a = await client.GetAsync(uri);
		return Compute(await a.Content.ReadAsByteArrayAsync());
	}

	public static byte[] Compute(byte[] data)
	{
		var hashAlgorithm = SHA1Managed.Create();
		hashAlgorithm.TransformFinalBlock(data);
		return hashAlgorithm.Hash;
	}

	public static async Task<byte[]> Compute(IUVStream stream)
	{
		var hashAlgorithm = SHA1Managed.Create();
		var buf = new ArraySegment<byte>(new byte[16 * 1024]);
		int n;
		while ((n = await stream.ReadAsync(buf)) > 0) {
			hashAlgorithm.TransformBlock(buf.Take(n));
		}
		hashAlgorithm.TransformFinalBlock();
		return hashAlgorithm.Hash;
	}

	static IEnumerable<string> files;

	public static async Task<byte[]> Differentiate(string argument)
	{
		if (files.Contains(argument)) {
			return await Compute(argument);
		} else {
			return await Compute(new Uri(argument));
		}
	}

	public static void Main(string[] args)
	{
		if (args.Length == 0) {
			Console.WriteLine("Provide some filenames in the arguments");
			return;
		}

		// initiating app state, so blocking calls are ok,
		// because we will wait for this in any case
		// in this small app
		files = new System.IO.DirectoryInfo("./").GetFiles().Select((di) => di.Name);

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		Loop.Default.Run(async () => {
			foreach (var file in await Task.WhenAll(args.Select(async (file) => Tuple.Create(file, await Differentiate(file))))) {
				Console.WriteLine("{1} {0}", file.Item1, file.Item2.ToHex());
			}
		});
		stopwatch.Stop();
		Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
	}
}
