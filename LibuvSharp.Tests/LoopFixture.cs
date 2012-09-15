using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class LoopFixture
	{
		[Test]
		public void NoAdditionalResources()
		{
			Loop.Default.Run();
		}

		[Test]
		public void RunAsync()
		{
			var timer = new Timer();
			Loop.Default.RunAsync();
			timer.Close();
		}

		[Test]
		public void Handles()
		{
			Assert.Greater(Loop.Default.Handles.Length, 0);
		}

		[Test]
		public void ActiveHandlesCount()
		{
			Assert.Greater(Loop.Default.ActiveHandlesCount, 0);
		}

		[Test]
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

