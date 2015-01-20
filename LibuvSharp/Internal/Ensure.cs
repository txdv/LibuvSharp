using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class Ensure
	{
		internal static Exception Map(int errorCode, string name = null)
		{
			return Map((uv_err_code)errorCode, name);
		}

		internal static Exception Map(uv_err_code error, string name = null)
		{
			if (error == uv_err_code.UV_OK) {
				return null;
			}

			switch (error) {
			case uv_err_code.UV_EINVAL:
				return new ArgumentException(UVException.StringError(error));
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

		public static void Success(int errorCode)
		{
			var e = Map((uv_err_code)errorCode);
			if (e != null) {
				throw e;
			}
		}

		internal static void Success(int errorCode, Action<Exception> callback, string name = null)
		{
			if (callback != null) {
				callback(Map(errorCode));
			}
		}

		internal static void Success<T>(Exception ex, Action<Exception, T> callback, T arg)
		{
			if (callback != null) {
				callback(ex, arg);
			}
		}

		public static void ArgumentNotNull(object argumentValue, string argumentName)
		{
			if (argumentValue == null) {
				throw new ArgumentNullException(argumentName);
			}
		}

		public static void AddressFamily(IPAddress ipAddress)
		{
			if ((ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) &&
			    (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)) {
				throw new ArgumentException("ipAddress has to be of AddressFamily InterNetwork or InterNetworkV6");
			}
		}
	}
}

