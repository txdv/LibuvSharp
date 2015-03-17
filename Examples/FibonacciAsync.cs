using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;

namespace Test
{
	class Job
	{
		public Task Task { get; private set; }
		public DateTime StartTime { get; private set; }
		public TimeSpan TimeRunning {
			get {
				return DateTime.Now - StartTime;
			}
		}

		public Job(Task task)
			: this(task, DateTime.Now)
		{
		}

		public Job(Task task, DateTime startTime)
		{
			Task = task;
			StartTime = startTime;
		}
	}

	class MainClass
	{
		/*
		 * An example which proves how wrong the 'node is cancer' article is about
		 * event loops. Well, the Fibonacci calculation is not executed in the
		 * event loop, but in a different thread, but the api exposed in the loop
		 * class makes it easy to utilize the thread pool in order to avoid blocking
		 * the main event loop with long calclulations.
		 * http://pages.citebite.com/b2x0j8q1megb
		 */

		public static BigInteger Fibonacci(int n)
		{
			switch (n) {
			case 0:
				return new BigInteger(0);
			case 1:
				return new BigInteger(1);
			default:
				return Fibonacci(n - 1) + Fibonacci(n - 2);
			}
		}

		public static async Task CalculateFibonacci(int n)
		{
			TimeSpan span = TimeSpan.Zero;
			BigInteger res = 0;
			await Task.Run(() => {
				var now = DateTime.Now;
				Console.WriteLine("fib({0}) started {1}", n, now);
				res = Fibonacci(n);
				span = DateTime.Now - now;
			});
			Console.WriteLine("{0}: fib({1}) = {2}", span, n, res);
		}

		public static async Task TrackCalculateFibonacci(LinkedList<Job> jobs, int n)
		{
			LinkedListNode<Job> node = null;
			try {
				node = jobs.AddLast(new Job(CalculateFibonacci(n)));
				await node.Value.Task;
			} finally {
				if (node != null) {
					jobs.Remove(node);
				}
			}
		}

		public static void Main(string[] args)
		{
			Loop.Default.Run(async () => {
				var jobs = new LinkedList<Job>();
				var stdin = new TTY(0);
				string str = null;
				while ((str = await stdin.ReadStringAsync()) != null) {
					str = str.TrimEnd(new char[] { '\r', '\n' });
					if (str == "quit") {
						Loop.Current.Stop();
						break;
					} else if (str.StartsWith("fib ")) {
						int n;
						if (!int.TryParse(str.Substring("fib ".Length), out n)) {
							Console.WriteLine("Supply an integer to the fib command");
							continue;
						}
						TrackCalculateFibonacci(jobs, n);
					} else if (str == "help") {
						Console.WriteLine("Available commands: ");
						Console.WriteLine("fib <n:int> - start a thread which calculates fib");
						Console.WriteLine("help - displays help");
						Console.WriteLine("quit - quits the program");
					} else if (str == "count") {
						Console.WriteLine("Total jobs: {0}", jobs.Count);
						int i = 0;
						foreach (var job in jobs) {
							var awaiter = job.Task.GetAwaiter();
							Console.WriteLine("{0}: {1} {2} {3}", i, job.StartTime, job.TimeRunning, job.Task.Status);
							i++;
						}
					} else {
						Console.WriteLine("Unknown command");
					}
				}
			});
		}
	}
}
