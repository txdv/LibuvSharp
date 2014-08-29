using System;
using LibuvSharp.Threading;
using Xunit;

namespace LibuvSharp.Tests
{
	public class ThreadingFixture
	{
		[Fact]
		public void LoopBlocking()
		{
			TimeSpan span = TimeSpan.Zero;
			Loop.Default.QueueUserWorkItem(() => {
				var now = DateTime.Now;
				System.Threading.Thread.Sleep(1000);
				span = DateTime.Now - now;
			}, null);
			Loop.Default.Run();

			Assert.NotEqual(span, TimeSpan.Zero);
			Assert.True(span.TotalMilliseconds >= 1000);
		}
	}
}

