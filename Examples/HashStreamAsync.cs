using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;
using LibuvSharp.Threading;

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

	public static async Task<byte[]> Compute(IUVStream<ArraySegment<byte>> stream)
	{
		var hashAlgorithm = SHA1Managed.Create();
		ArraySegment<byte>? data;
		while ((data = await stream.ReadStructAsync()).HasValue) {
			hashAlgorithm.TransformBlock(data.Value);
		}
		hashAlgorithm.TransformFinalBlock();
		return hashAlgorithm.Hash;
	}

	public static async Task<byte[]> Differentiate(string argument)
	{
		try {
			return await Compute(new Uri(argument));
		} catch {
			return await Compute(argument);
		}
	}

	public static void Main(string[] args)
	{
		if (args.Length == 0) {
			Console.WriteLine("Provide some filenames in the arguments");
			return;
		}

		var files = args.Where(arg => File.GetAttributes(arg) == FileAttributes.Normal);

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		Loop.Default.Run(async () => {
			foreach (var file in await Task.WhenAll(files.Select(async (file) => Tuple.Create(file, await Differentiate(file))))) {
				Console.WriteLine("{1} {0}", file.Item1, file.Item2.ToHex());
			}
		});
		stopwatch.Stop();
		Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
	}
}
