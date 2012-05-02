using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
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
			if (error.code == uv_err_code.UV_OK) {
				return;
			}
			switch (error.code) {
			case uv_err_code.UV_EADDRINUSE:
				throw new SocketException(10048);
			default:
				throw new UVException(error);

			}
		}

		internal static void Success(int errorCode, Loop loop)
		{
			if (errorCode < 0) {
				Success(uv_last_error(loop.Handle));
			}
		}

	}
}

