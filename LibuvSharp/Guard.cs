using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public class Guard : BaseHandle
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal extern static void uv_once(IntPtr ptr, Action callback);

		public Guard()
			: base(5)
		{
		}

		public void Once(Action callback)
		{
			var cb = new CAction(callback);
			uv_once(Handle, cb.Callback);
		}
	}
}

