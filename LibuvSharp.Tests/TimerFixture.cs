using System;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class TimerFixture
	{
		[Test]
		public static void Simple()
		{
			Simple(10, 10);
			Simple(2, 50);
			Simple(50, 1);
		}

		public static void Simple(int times, int spawn)
		{
			var t = new UVTimer();
			int i = 0;
			t.Tick +=  () => {
				i++;
				if (i > times) {
					t.Close();
				}
			};
			t.Start(TimeSpan.FromMilliseconds(spawn));
			var now = Loop.Default.Now;
			Loop.Default.Run();
			Assert.GreaterOrEqual(Loop.Default.Now - now, times * spawn);
			Assert.IsTrue(t.IsClosed);
		}
	}
}

