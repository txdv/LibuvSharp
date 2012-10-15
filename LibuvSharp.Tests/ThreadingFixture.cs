using System;
using LibuvSharp.Threading;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class ThreadingFixture
	{
		[Test]
		public void LoopBlocking()
		{
			TimeSpan span = TimeSpan.Zero;
			Loop.Default.QueueUserWorkItem(() => {
				var now = DateTime.Now;
				System.Threading.Thread.Sleep(1000);
				span = DateTime.Now - now;
			}, null);
			Loop.Default.Run();

			Assert.AreNotEqual(span, TimeSpan.Zero);
			Assert.GreaterOrEqual(span.TotalMilliseconds, 1000);
		}
	}
}

