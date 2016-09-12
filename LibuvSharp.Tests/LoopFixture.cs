using System;
using Xunit;

namespace LibuvSharp.Tests
{
	public class LoopFixture : Fixture
	{
		[Fact]
		public void NoAdditionalResources()
		{
			Loop.Current.Run();
		}

		[Fact]
		public void RunAsync()
		{
			var timer = new UVTimer();
			Loop.Current.RunAsync();
			timer.Close();
		}

		[Fact]
		public void Handles()
		{
			Assert.True(Loop.Current.Handles.Length > 0);
		}

		public void ActiveHandlesCount()
		{
			Assert.True(Loop.Current.ActiveHandlesCount > 0);
		}

		[Fact]
		public void Data()
		{
			Assert.True(Loop.Current.Data == IntPtr.Zero);
			Loop.Current.Data = new IntPtr(42);
			Assert.Equal(Loop.Current.Data.ToInt32(), 42);
			Loop.Current.Data = IntPtr.Zero;
			Assert.True(Loop.Current.Data == IntPtr.Zero);
		}
	}
}

