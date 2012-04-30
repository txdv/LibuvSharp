using System;
using NUnit.Framework;
using Libuv;

namespace Test
{
	[TestFixture]
	public class TimerTest
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
			Timer t = new Timer();
			int i = 0;
			t.Start(TimeSpan.FromMilliseconds(spawn), (_) => {
				i++;
				if (i > times) {
					t.Close();
				}
			});
			var now = Loop.Default.Now;
			Loop.Default.Run();
			Assert.GreaterOrEqual(Loop.Default.Now - now, times * spawn);
			Assert.IsTrue(t.Closed);
		}
	}
}

