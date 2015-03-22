using System;
using System.Linq;
using System.Security.Cryptography;
using System.Diagnostics;
using LibuvSharp;

public class HashStream : IDisposable
{
	public HashAlgorithm HashAlgorithm { get; private set; }
	public IUVStream<ArraySegment<byte>> Stream { get; private set; }
	public byte[] Hash { get; private set; }
	public string HashString {
		get {
			return string.Join(string.Empty, Hash.Select((x) => x.ToString("x2")));
		}
	}

	public HashStream(HashAlgorithm algorithm, IUVStream<ArraySegment<byte>> stream)
	{
		HashAlgorithm = algorithm;
		Stream = stream;

		Stream.Data += ((b) => {
			HashAlgorithm.TransformBlock(b.Array, b.Offset, b.Count, null, 0);
		});

		Stream.Complete += () => {
			HashAlgorithm.TransformFinalBlock(new byte[] { }, 0, 0);
			Hash = HashAlgorithm.Hash;
			if (Complete != null) {
				Complete();
			}
		};
	}

	public event Action Complete;

	public void Dispose()
	{
		if (HashAlgorithm is IDisposable) {
			(HashAlgorithm as IDisposable).Dispose();
			HashAlgorithm = null;
		}
	}

	public static void Compute(HashAlgorithm hashAlgorithm, IUVStream<ArraySegment<byte>> stream, Action<byte[]> callback)
	{
		var hs = new HashStream(hashAlgorithm, stream);
		hs.Complete += () => {
			callback(hs.Hash);
			hs.Dispose();
		};
	}

	public static void ComputeString(HashAlgorithm hashAlgorithm, IUVStream<ArraySegment<byte>> stream, Action<string> callback)
	{
		var hs = new HashStream(hashAlgorithm, stream);
		hs.Complete += () => {
			callback(hs.HashString);
			hs.Dispose();
		};
	}

	public static void Compute(Loop loop, string file)
	{
		var @in = new UVFileStream(loop);
		@in.Open(file, UVFileAccess.Read, (ex) => {
			HashStream.ComputeString(SHA1Managed.Create(), @in, (str) => {
				Console.WriteLine ("{0} {1}", str, file);
			});
			@in.Resume();
		});
	}

	public static void Main(string[] args)
	{
		if (args.Length == 0) {
			Console.WriteLine("Provide some filenames in the arguments");
			return;
		}

		foreach (var arg in args) {
			Compute(Loop.Default, arg);
		}

		var stopwatch = Stopwatch.StartNew();
		Loop.Default.Run();
		Console.WriteLine(stopwatch.Elapsed.TotalMilliseconds);
	}
}
