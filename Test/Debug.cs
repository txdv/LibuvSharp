using System;

namespace Test
{
	public static class Debug
	{
		public static void Assert(bool result)
		{
			if (!result) {
				throw new Exception("debug assert");
			}
		}

		public static void Assert(object o1, object o2)
		{
			Debug.Assert(o1.Equals(o2));
		}
	}
}

