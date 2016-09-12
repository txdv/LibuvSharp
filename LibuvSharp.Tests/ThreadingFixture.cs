using System;
using System.Diagnostics;
using LibuvSharp.Threading;
using Xunit;

namespace LibuvSharp.Tests
{
	public class ThreadingFixture : Fixture
	{
		[Fact]
		public void LoopBlocking()
		{
			TimeSpan span = TimeSpan.Zero;
			Loop.Current.QueueUserWorkItem(() => {
				var stopwatch = Stopwatch.StartNew();
				System.Threading.Thread.Sleep(1000);
				stopwatch.Stop();
				span = stopwatch.Elapsed;
			}, null);
			Loop.Current.Run();

			Assert.NotEqual(span, TimeSpan.Zero);
			Assert.True(span.TotalMilliseconds >= 1000);
		}
	}
}

