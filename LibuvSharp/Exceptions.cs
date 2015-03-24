using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe public class UVException : Exception
	{
		/// <summary>
		/// Independent error code, has the same value on all
		/// systems.
		/// </summary>
		/// <value>The error code.</value>
		internal UVErrorCode ErrorCode { get; set; }

		/// <summary>
		/// Gets the the underlying system error code of the error
		/// They might be different on windows and unix, EAGAIN
		/// for example is -4088 on windows while it is -11 on UNIX.
		/// </summary>
		/// <value>The system error code.</value>
		public int SystemErrorCode { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }

		public UVException(int systemErrorCode, string name, string description)
			: base(string.Format("{0}({1}): {2}", name, systemErrorCode, description))
		{
			ErrorCode = Map(name);
			SystemErrorCode = systemErrorCode;
			Name = name;
			Description = description;
		}

		internal UVException(int systemErrorCode)
			: this(systemErrorCode, ErrorName(systemErrorCode), StringError(systemErrorCode))
		{
		}


		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_strerror(int systemErrorCode);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_err_name(int systemErrorCode);

		internal static string StringError(int systemErrorCode)
		{
			return new string(uv_strerror(systemErrorCode));
		}

		internal static string ErrorName(int systemErrorCode)
		{
			return new string(uv_err_name(systemErrorCode));
		}

		public static UVErrorCode Map(int systemErrorCode)
		{
			if (systemErrorCode == 0) {
				return UVErrorCode.OK;
			}
			return Map(ErrorName(systemErrorCode));
		}

		public static UVErrorCode Map(string errorName)
		{
			try {
				return (UVErrorCode)Enum.Parse(typeof(UVErrorCode), errorName);
			} catch {
				return UVErrorCode.UNKNOWN;
			}
		}

		/// <summary>
		/// Returns the corresponding SocketError.
		/// </summary>
		/// <value>The socket error.</value>
		public SocketError SocketError {
			get {
				// every comment prefixed with WSA is not in the reference source
				// every comment prefixed with SocktError is not defined in uv.h
				switch (ErrorCode) {
				case UVErrorCode.EINTR:
					return SocketError.Interrupted;
				case UVErrorCode.EACCES:
					return SocketError.AccessDenied;
				case UVErrorCode.EFAULT:
					return SocketError.Fault;
				case UVErrorCode.EINVAL:
					return SocketError.InvalidArgument;
				case UVErrorCode.EMFILE:
					return SocketError.TooManyOpenSockets;
				case UVErrorCode.EAGAIN:
					return SocketError.WouldBlock;
				case UVErrorCode.EALREADY:
					return SocketError.AlreadyInProgress;
				case UVErrorCode.ENOTSOCK:
					return SocketError.NotSocket;
				case UVErrorCode.EDESTADDRREQ:
					return SocketError.DestinationAddressRequired;
				case UVErrorCode.EMSGSIZE:
					return SocketError.MessageSize;
				case UVErrorCode.EPROTOTYPE:
					return SocketError.ProtocolType;
				// SocketError.ProtocolOption
				case UVErrorCode.EPROTONOSUPPORT:
					return SocketError.ProtocolNotSupported;
				// SocketNotSupported;
				case UVErrorCode.ENOTSUP:
					return SocketError.OperationNotSupported;
				// SocketError.ProtocolFamilyNotSupported
				case UVErrorCode.EAFNOSUPPORT:
					return SocketError.AddressFamilyNotSupported;
				case UVErrorCode.EADDRINUSE:
					return SocketError.AddressAlreadyInUse;
				case UVErrorCode.EADDRNOTAVAIL:
					return SocketError.AddressNotAvailable;
				case UVErrorCode.ENETDOWN:
					return SocketError.NetworkDown;
				case UVErrorCode.ENETUNREACH:
					return SocketError.NetworkUnreachable;
				// SocketError.NetworkReset
				case UVErrorCode.ECONNABORTED:
					return SocketError.ConnectionAborted;
				case UVErrorCode.ECONNRESET:
					return SocketError.ConnectionReset;
				case UVErrorCode.ENOBUFS:
					return SocketError.NoBufferSpaceAvailable;
				case UVErrorCode.EISCONN:
					return SocketError.IsConnected;
				case UVErrorCode.ENOTCONN:
					return SocketError.NotConnected;
				case UVErrorCode.ESHUTDOWN:
					return SocketError.Shutdown;
				case UVErrorCode.ETIMEDOUT:
					return SocketError.TimedOut;
				case UVErrorCode.ECONNREFUSED:
					return SocketError.ConnectionRefused;
				// WSAELOOP
				// WSAENAMETOOLONG
				// SocketError.HostDown
				case UVErrorCode.EHOSTUNREACH:
					return SocketError.HostUnreachable;
				// WSAENOTEMPTY
				// SocketError.ProcessLimit
				// WSAEUSERS
				// WSAEDQUOT
				// WSAESTALE

				// Windows only error codes:

				// WSASYSNOTREADY
				// SocketError.SystemNotReady

				// WSAVERNOTSUPPORTED
				// SocketError.VersionNotSupported

				// WSANOTINITIALISED
				// SocketError.NotInitialized

				// WSAEDISCON
				// SocketError.Disconnecting

				// WSATYPE_NOT_FOUND
				// SocketError.TypeNotFound

				// WSAHOST_NOT_FOUND
				// SocketError.HostNotFound

				// WSATRY_AGAIN
				// SocketError.TryAgain

				// WSANO_RECOVERY
				// SocketError.NoRecovery

				// WSANO_DATA
				// SocketError.NoData

				// os, overlapped:

				// SocketError.IOPending
				// SocketError.OperationAborted
				default:
					return SocketError.SocketError;
				}
			}
		}
	}
}

