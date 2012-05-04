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
			IntPtr ptr;

			Assert.Throws<System.IO.FileNotFoundException>(() => DynamicLibrary.Open("NOT_EXISTING"));

			string failure = "FAILURE";
			var fs = new System.IO.StreamWriter(System.IO.File.OpenWrite(failure));
			fs.Write("foobar");
			fs.Close();

			Assert.IsTrue(System.IO.File.Exists(failure));
			Assert.Throws<System.IO.FileNotFoundException>(() => DynamicLibrary.Open(failure));

			System.IO.File.Delete(failure);


			var dl = DynamicLibrary.Open(DynamicLibrary.Decorate("uv"));

			Assert.IsTrue(dl.TryGetSymbol("uv_default_loop", out ptr));
			Assert.AreNotEqual(ptr, IntPtr.Zero);

			Assert.IsFalse(dl.TryGetSymbol("NOT_EXISTING", out ptr));
			Assert.AreEqual(ptr, IntPtr.Zero);

			Assert.IsFalse(dl.Closed);
			dl.Close();
			Assert.IsTrue(dl.Closed);
		}
	}
}

