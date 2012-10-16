using System;
using System.Linq;
using System.Security.Cryptography;
using LibuvSharp;

public class HashStream : IDisposable
{
	public HashAlgorithm HashAlgorithm { get; private set; }
	public IUVStream Stream { get; private set; }
	public byte[] Hash { get; private set; }
	public string HashString {
		get {
			return string.Join(string.Empty, Hash.Select((x) => x.ToString("x2")));
		}
	}

	public HashStream(HashAlgorithm algorithm, IUVStream stream)
	{
		HashAlgorithm = algorithm;
		Stream = stream;

		Stream.Data += ((b) => {
			HashAlgorithm.TransformBlock(b.Buffer, b.Start, b.Length, null, 0);
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

	public static void Compute(HashAlgorithm hashAlgorithm, IUVStream stream, Action<byte[]> callback)
	{
		var hs = new HashStream(hashAlgorithm, stream);
		hs.Complete += () => {
			callback(hs.Hash);
			hs.Dispose();
		};
	}

	public static void ComputeString(HashAlgorithm hashAlgorithm, IUVStream stream, Action<string> callback)
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
		int n;
		if (args.Length > 1 && int.TryParse(args[1], out n)) {
		} else {
			n = 1;
		}

		for (int i = 0; i < n; i++) {
			Compute(Loop.Default, args[0]);
		}
		var now = DateTime.Now;
		Loop.Default.Run();
		Console.WriteLine ((DateTime.Now - now).TotalMilliseconds);
	}
}
