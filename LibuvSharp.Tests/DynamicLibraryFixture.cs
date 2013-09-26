using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class DynamicLibraryFixture
	{
		[TestCase]
		public void Error()
		{
			IntPtr ptr;

			Assert.Throws<Exception>(() => DynamicLibrary.Open("NOT_EXISTING"));

			string failure = "FAILURE";
			var fs = new System.IO.StreamWriter(System.IO.File.OpenWrite(failure));
			fs.Write("foobar");
			fs.Close();

			Assert.IsTrue(System.IO.File.Exists(failure));
			Assert.Throws<Exception>(() => DynamicLibrary.Open(failure));

			System.IO.File.Delete(failure);


			var dl = DynamicLibrary.Open(DynamicLibrary.Decorate("uv"));

			Assert.IsTrue(dl.TryGetSymbol("uv_default_loop", out ptr));
			Assert.AreNotEqual(ptr, IntPtr.Zero);

			Assert.IsFalse(dl.TryGetSymbol("NOT_EXISTING", out ptr));
			Assert.AreEqual(ptr, IntPtr.Zero);

			Assert.Throws<Exception>(() => dl.GetSymbol("NOT_EXISTING"));

			Assert.IsFalse(dl.Closed);
			dl.Close();
			Assert.IsTrue(dl.Closed);
		}
	}
}

