using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	internal class Ensure
	{
		internal static Exception Map(int systemErrorCode, string name = null)
		{
			if (systemErrorCode < 0) {
				return Map(new UVException(systemErrorCode), name);
			}

			return null;
		}

		internal static Exception Map(UVException exception, string name = null)
		{
			if (exception.ErrorCode == UVErrorCode.OK) {
				return null;
			}

			switch (exception.ErrorCode) {
			case UVErrorCode.EINVAL:
				return new ArgumentException(exception.Description);
			case UVErrorCode.ENOENT:
				var path = (name == null ? System.IO.Directory.GetCurrentDirectory() : Path.Combine(System.IO.Directory.GetCurrentDirectory(), name));
				return new System.IO.FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
			case UVErrorCode.EADDRINUSE:
				return new SocketException(10048);
			case UVErrorCode.EADDRNOTAVAIL:
				return new SocketException(10049);
			case UVErrorCode.ECONNREFUSED:
				return new SocketException(10061);
			case UVErrorCode.ENOTSUP:
				return new NotSupportedException();
			default:
				break;
			}

			return exception;
		}

		public static void Success(int errorCode)
		{
			if (errorCode < 0) {
				var e = Map(errorCode);
				if (e != null) {
					throw e;
				}
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

