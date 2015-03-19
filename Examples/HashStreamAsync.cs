using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
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

	public static void Main(string[] args)
	{
		if (args.Length == 0) {
			Console.WriteLine("Provide some filenames in the arguments");
			return;
		}

		var now = DateTime.Now;
		Loop.Default.Run(async () => {
			foreach (var file in await Task.WhenAll(args.Select(async (file) => Tuple.Create(file, await Compute(file))))) {
				Console.WriteLine("{1} {0}", file.Item1, file.Item2.ToHex());
			}
		});
		Console.WriteLine((DateTime.Now - now).TotalMilliseconds);
	}
}
