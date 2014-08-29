using System;
using Xunit;

namespace LibuvSharp.Tests
{
	public class DynamicLibraryFixture
	{
		[Fact]
		public void Error()
		{
			IntPtr ptr;

			Assert.Throws<Exception>(() => DynamicLibrary.Open("NOT_EXISTING"));

			string failure = "FAILURE";
			var fs = new System.IO.StreamWriter(System.IO.File.OpenWrite(failure));
			fs.Write("foobar");
			fs.Close();

			Assert.True(System.IO.File.Exists(failure));
			Assert.Throws<Exception>(() => DynamicLibrary.Open(failure));

			System.IO.File.Delete(failure);


			var dl = DynamicLibrary.Open(DynamicLibrary.Decorate("uv"));

			Assert.True(dl.TryGetSymbol("uv_default_loop", out ptr));
			Assert.NotEqual(ptr, IntPtr.Zero);

			Assert.False(dl.TryGetSymbol("NOT_EXISTING", out ptr));
			Assert.Equal(ptr, IntPtr.Zero);

			Assert.Throws<Exception>(() => dl.GetSymbol("NOT_EXISTING"));

			Assert.False(dl.Closed);
			dl.Close();
			Assert.True(dl.Closed);
		}
	}
}

