using System;
using Xunit;

namespace LibuvSharp.Tests
{
	public class LoopFixture
	{
		[Fact]
		public void NoAdditionalResources()
		{
			Loop.Default.Run();
		}

		[Fact]
		public void RunAsync()
		{
			var timer = new UVTimer();
			Loop.Default.RunAsync();
			timer.Close();
		}

		[Fact]
		public void Handles()
		{
			Assert.True(Loop.Default.Handles.Length > 0);
		}

		public void ActiveHandlesCount()
		{
			Assert.True(Loop.Default.ActiveHandlesCount > 0);
		}

		[Fact]
		public void Data()
		{
			Assert.True(Loop.Default.Data == IntPtr.Zero);
			Loop.Default.Data = new IntPtr(42);
			Assert.Equal(Loop.Default.Data.ToInt32(), 42);
			Loop.Default.Data = IntPtr.Zero;
			Assert.True(Loop.Default.Data == IntPtr.Zero);
		}
	}
}

