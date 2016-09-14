using System;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IHandleExtensions
	{
		public static Task CloseAsync(this IHandle handle)
		{
			return HelperFunctions.WrapSingle(handle.Close);
		}
	}
}

