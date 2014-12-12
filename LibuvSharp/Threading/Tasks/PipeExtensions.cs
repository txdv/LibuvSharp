using System;
using System.Threading.Tasks;

namespace LibuvSharp
{
	public static class PipeExtensions
	{
		public static Task ConnectAsync(this Pipe pipe, string name)
		{
			return HelperFunctions.Wrap(name, pipe.Connect);
		}
	}
}

