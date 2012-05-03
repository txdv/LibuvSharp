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
			Assert.Throws<ArgumentNullException>(() => new DynamicLibrary(null));
			Assert.Throws<ArgumentException>(() => new DynamicLibrary("NOT_EXISTING"));

			var dl = new DynamicLibrary();
			IntPtr ptr;
			Assert.IsTrue(dl.TryGetSymbol("strlen", out ptr));
			Assert.IsFalse(dl.TryGetSymbol("NOT_EXISTING", out ptr));

			Assert.AreNotEqual(dl.GetSymbol("strlen"), IntPtr.Zero);
			Assert.Throws<ArgumentException>(() => dl.GetSymbol("NOT_EXISTING"));

			Assert.IsFalse(dl.Closed);
			dl.Close();
			Assert.IsTrue(dl.Closed);
		}
	}
}

