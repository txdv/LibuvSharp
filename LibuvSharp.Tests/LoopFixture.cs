using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class LoopFixture
	{
		[TestCase]
		public void NoAdditionalResources()
		{
			Loop.Default.Run();
		}

		[TestCase]
		public void RunAsync()
		{
			var timer = new UVTimer();
			Loop.Default.RunAsync();
			timer.Close();
		}

		[TestCase]
		public void Handles()
		{
			Assert.Greater(Loop.Default.Handles.Length, 0);
		}

		public void ActiveHandlesCount()
		{
			Assert.Greater(Loop.Default.ActiveHandlesCount, 0);
		}

		[TestCase]
		public void Data()
		{
			Assert.IsTrue(Loop.Default.Data == IntPtr.Zero);
			Loop.Default.Data = new IntPtr(42);
			Assert.AreEqual(Loop.Default.Data.ToInt32(), 42);
			Loop.Default.Data = IntPtr.Zero;
			Assert.IsTrue(Loop.Default.Data == IntPtr.Zero);
		}
	}
}

