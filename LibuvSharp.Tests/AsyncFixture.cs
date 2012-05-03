using System;
using System.Collections.Generic;
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

			Loop.Default.RunOnce();

			async.Close();
			asyncWatcher.Close ();
		}
	}
}

