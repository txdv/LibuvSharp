using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Xunit;


namespace LibuvSharp.Tests
{

	public class Fixture : IDisposable
	{
		private Loop lastLoop;

		string Identifier => string.Join(string.Empty, BitConverter.GetBytes(this.GetHashCode()).Select(i => i.ToString("x2")));

		static bool Debug => System.Environment.GetEnvironmentVariable("DEBUG") != null;

		static object debug = new object();

		public DateTime StartTime { get; private set; }
		public DateTime EndTime { get; private set; }

		public Fixture()
		{
			StartTime = DateTime.Now;
			lastLoop = Loop.Current;
			Loop.Current = new Loop();
			if (Debug) {
				lock (debug) {
					Console.WriteLine("{0}({1}) Start {2}", this.GetType(), Identifier, DateTime.Now);
				}
			}
		}

		public void Dispose()
		{
			Loop.Current.Dispose();
			Loop.Current = lastLoop;
			if (Debug) {
				lock (debug) {
					Console.WriteLine("{0}({1}) End {2}", this.GetType(), Identifier, DateTime.Now);
				}
			}
		}
	}
}

