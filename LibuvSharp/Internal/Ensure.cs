using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class Ensure
	{
		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		internal static extern uv_err_t uv_last_error(IntPtr loop);

		internal static Exception Map(uv_err_t error, string name = null)
		{
			if (error.code == uv_err_code.UV_OK) {
				return null;
			}

			switch (error.code) {
			case uv_err_code.UV_EINVAL:
				return new ArgumentException(error.Description);
			case uv_err_code.UV_ENOENT:
				var path = (name == null ? System.IO.Directory.GetCurrentDirectory() : Path.Combine(System.IO.Directory.GetCurrentDirectory(), name));
				return new System.IO.FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
			case uv_err_code.UV_EADDRINUSE:
				return new SocketException(10048);
			case uv_err_code.UV_EADDRNOTAVAIL:
				return new SocketException(10049);
			case uv_err_code.UV_ECONNREFUSED:
				return new SocketException(10061);
			default:
				return new UVException(error);
			}
		}

		internal static void Success(int errorCode)
		{
			if (errorCode < 0) {
				throw new Exception(errorCode.ToString());
			}
		}

		internal static void Success(uv_err_t error)
		{
			var e = Map(error);
			if (e != null) {
				throw e;
			}
		}

		internal static void Success(int errorCode, Loop loop)
		{
			if (errorCode < 0) {
				throw Success(loop);
			}
		}

		internal static Exception Success(Loop loop, string name = null)
		{
			return Map(uv_last_error(loop.NativeHandle), name);
		}

		public static void ArgumentNotNull(object argumentValue, string argumentName)
		{
			if (argumentValue == null) {
				throw new ArgumentNullException(argumentName);
			}
		}
	}
}

