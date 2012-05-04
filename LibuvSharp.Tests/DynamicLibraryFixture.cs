using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class DynamicLibraryFixture
	{
		[Test]
		public static void Error()
		{
			// TODO: if library or function does not exist, I get AccessViolationErrors ...

			IntPtr ptr;
			string lib;
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				lib = "libuv.so";
			} else {
				lib = "uv.dll";
			}

			var dl = new DynamicLibrary(lib);

			Assert.IsTrue(dl.TryGetSymbol("uv_default_loop", out ptr));
			Assert.IsNotNull(ptr);

			Assert.IsFalse(dl.Closed);
			dl.Close();
			Assert.IsTrue(dl.Closed);
		}
	}
}

