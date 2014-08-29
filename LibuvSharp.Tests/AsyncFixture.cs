using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace LibuvSharp.Tests
{
	public class AsyncFixture
	{
		[Fact]
		public void ArgumentNullExceptions()
		{
			Assert.Throws<ArgumentNullException>(() => new Async(null as Loop));
			Assert.Throws<ArgumentNullException>(() => new AsyncWatcher<int>(null as Loop));

			var async = new Async();
			var asyncWatcher = new AsyncWatcher<int>();

			Assert.Throws<ArgumentNullException>(() => asyncWatcher.Send(null as IEnumerable<int>));

			async.Close();
			asyncWatcher.Close ();
		}

		[Fact]
		public void Simple()
		{
			int async_cb_called = 0;
			object o = new object();
			var async = new Async();
			async.Callback += (a) => {
				int n;
				lock (o) {
					n = ++async_cb_called;
				}

				if (n == 3) {
					a.Close();
				}
			};

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

