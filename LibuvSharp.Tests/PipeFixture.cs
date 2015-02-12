using System;
using System.Net;
using System.Text;
using Xunit;

namespace LibuvSharp.Tests
{
	public class PipeFixture
	{
		[Fact]
		public void Simple()
		{
			Default.SimpleTest<string, PipeListener, Pipe>(Default.Pipename);

			Loop.Default.Run(async () => await Default.SimpleTestAsync<string, PipeListener, Pipe>(Default.Pipename));
		}

		[Fact]
		public void Stress()
		{
			Default.StressTest<string, PipeListener, Pipe>(Default.Pipename);
		}

		[Fact]
		public void OneSideClose()
		{
			Default.OneSideCloseTest<string, PipeListener, Pipe>(Default.Pipename);
		}

		[Fact]
		public void ConnectToNotListeningFile()
		{
			Pipe pipe = new Pipe();
			pipe.Connect("NOT_EXISTING", (e) => {
				Assert.NotNull(e);
				Assert.IsType<System.IO.FileNotFoundException>(e);
			});
			Loop.Default.Run();
		}

		[Fact]
		public void NotNullListener()
		{
			var t = new PipeListener();
			Assert.Throws<ArgumentNullException>(() => new PipeListener(null));
			Assert.Throws<ArgumentNullException>(() => t.Bind(null));
			t.Close();
		}
	}
}

