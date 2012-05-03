using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace LibuvSharp.Tests
{
	[TestFixture]
	public class AsyncFixture
	{
		[Test]
		public static void ArgumentNullExceptions()
		{
			Assert.Throws<ArgumentNullException>(() => new Async(null as Loop));
			Assert.Throws<ArgumentNullException>(() => new AsyncWatcher<int>(null as Loop));

			var async = new Async();
			var asyncWatcher = new AsyncWatcher<int>();

			Assert.Throws<ArgumentNullException>(() => asyncWatcher.Send(null as IEnumerable<int>));

			async.Close();
			asyncWatcher.Close ();
		}

		[Test]
		public static void Simple()
		{
			int async_cb_called = 0;
			object o = new object();
			var async = new Async((a) => {
				int n;
				lock (o) {
					n = ++async_cb_called;
				}

				if (n == 3) {
					a.Close();
				}
			});

			new Thread(() => {
				while (true) {
					int n;
					lock (o) {
						n = async_cb_called;
					}

					if (n == 3) {
						break;
					}

					async.Send();
				}
			}).Start();

			Loop.Default.Run();
		}
	}
}

