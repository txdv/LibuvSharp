using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	internal class Ensure
	{
		[DllImport("uv")]
		public static extern uv_err_t uv_last_error(IntPtr loop);

		internal static void Success(int errorCode)
		{
			if (errorCode < 0) {
				throw new Exception(errorCode.ToString());
			}
		}

		internal static void Success(uv_err_t error)
		{
			if (error.code != 0) {
				throw new UVException(error);
			}
		}

		internal static void Success(uv_err_code code)
		{
			if (code != uv_err_code.UV_OK) {
				throw new Exception(string.Format("{0}:{1}", (int)code, code));
			}
		}

		internal static void Success(int errorCode, Loop loop)
		{
			if (errorCode < 0) {
				throw new UVException(loop);
			}
		}

	}
}

