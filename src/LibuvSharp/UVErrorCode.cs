using System;

namespace LibuvSharp
{
	public enum UVErrorCode
	{
		OK = 0,
		/// <summary>
		/// argument list too long
		/// </summary>
		E2BIG,
		/// <summary>
		/// permission denied
		/// </summary>
		EACCES,
		/// <summary>
		/// address already in use
		/// </summary>
		EADDRINUSE,
		/// <summary>
		/// address not available
		/// </summary>
		EADDRNOTAVAIL,
		/// <summary>
		/// address family not supported
		/// </summary>
		EAFNOSUPPORT,
		/// <summary>
		/// resource temporarily unavailable
		/// </summary>
		EAGAIN,
		/// <summary>
		/// address family not supported
		/// </summary>
		EAI_ADDRFAMILY,
		/// <summary>
		/// temporary failure
		/// </summary>
		EAI_AGAIN,
		/// <summary>
		/// bad ai_flags value
		/// </summary>
		EAI_BADFLAGS,
		/// <summary>
		/// invalid value for hints
		/// </summary>
		EAI_BADHINTS,
		/// <summary>
		/// request canceled
		/// </summary>
		EAI_CANCELED,
		/// <summary>
		/// permanent failure
		/// </summary>
		EAI_FAIL,
		/// <summary>
		/// ai_family not supported
		/// </summary>
		EAI_FAMILY,
		/// <summary>
		/// out of memory
		/// </summary>
		EAI_MEMORY,
		/// <summary>
		/// no address
		/// </summary>
		EAI_NODATA,
		/// <summary>
		/// unknown node or service
		/// </summary>
		EAI_NONAME,
		/// <summary>
		/// argument buffer overflow
		/// </summary>
		EAI_OVERFLOW,
		/// <summary>
		/// resolved protocol is unknown
		/// </summary>
		EAI_PROTOCOL,
		/// <summary>
		/// service not available for socket type
		/// </summary>
		EAI_SERVICE,
		/// <summary>
		/// socket type not supported
		/// </summary>
		EAI_SOCKTYPE,
		/// <summary>
		/// connection already in progress
		/// </summary>
		EALREADY,
		/// <summary>
		/// bad file descriptor
		/// </summary>
		EBADF,
		/// <summary>
		/// resource busy or locked
		/// </summary>
		EBUSY,
		/// <summary>
		/// operation canceled
		/// </summary>
		ECANCELED,
		/// <summary>
		/// invalid Unicode character
		/// </summary>
		ECHARSET,
		/// <summary>
		/// software caused connection abort
		/// </summary>
		ECONNABORTED,
		/// <summary>
		/// connection refused
		/// </summary>
		ECONNREFUSED,
		/// <summary>
		/// connection reset by peer
		/// </summary>
		ECONNRESET,
		/// <summary>
		/// destination address required
		/// </summary>
		EDESTADDRREQ,
		/// <summary>
		/// file already exists
		/// </summary>
		EEXIST,
		/// <summary>
		/// bad address in system call argument
		/// </summary>
		EFAULT,
		/// <summary>
		/// file too large
		/// </summary>
		EFBIG,
		/// <summary>
		/// host is unreachable
		/// </summary>
		EHOSTUNREACH,
		/// <summary>
		/// interrupted system call
		/// </summary>
		EINTR,
		/// <summary>
		/// invalid argument
		/// </summary>
		EINVAL,
		/// <summary>
		/// i/o error
		/// </summary>
		EIO,
		/// <summary>
		/// socket is already connected
		/// </summary>
		EISCONN,
		/// <summary>
		/// illegal operation on a directory
		/// </summary>
		EISDIR,
		/// <summary>
		/// too many symbolic links encountered
		/// </summary>
		ELOOP,
		/// <summary>
		/// too many open files
		/// </summary>
		EMFILE,
		/// <summary>
		/// message too long
		/// </summary>
		EMSGSIZE,
		/// <summary>
		/// name too long
		/// </summary>
		ENAMETOOLONG,
		/// <summary>
		/// network is down
		/// </summary>
		ENETDOWN,
		/// <summary>
		/// network is unreachable
		/// </summary>
		ENETUNREACH,
		/// <summary>
		/// file table overflow
		/// </summary>
		ENFILE,
		/// <summary>
		/// no buffer space available
		/// </summary>
		ENOBUFS,
		/// <summary>
		/// no such device
		/// </summary>
		ENODEV,
		/// <summary>
		/// no such file or directory
		/// </summary>
		ENOENT,
		/// <summary>
		/// not enough memory
		/// </summary>
		ENOMEM,
		/// <summary>
		/// machine is not on the network
		/// </summary>
		ENONET,
		/// <summary>
		/// protocol not available
		/// </summary>
		ENOPROTOOPT,
		/// <summary>
		/// no space left on device
		/// </summary>
		ENOSPC,
		/// <summary>
		/// function not implemented
		/// </summary>
		ENOSYS,
		/// <summary>
		/// socket is not connected
		/// </summary>
		ENOTCONN,
		/// <summary>
		/// not a directory
		/// </summary>
		ENOTDIR,
		/// <summary>
		/// directory not empty
		/// </summary>
		ENOTEMPTY,
		/// <summary>
		/// socket operation on non-socket
		/// </summary>
		ENOTSOCK,
		/// <summary>
		/// operation not supported on socket
		/// </summary>
		ENOTSUP,
		/// <summary>
		/// operation not permitted
		/// </summary>
		EPERM,
		/// <summary>
		/// broken pipe
		/// </summary>
		EPIPE,
		/// <summary>
		/// protocol error
		/// </summary>
		EPROTO,
		/// <summary>
		/// protocol not supported
		/// </summary>
		EPROTONOSUPPORT,
		/// <summary>
		/// protocol wrong type for socket
		/// </summary>
		EPROTOTYPE,
		/// <summary>
		/// result too large
		/// </summary>
		ERANGE,
		/// <summary>
		/// read-only file system
		/// </summary>
		EROFS,
		/// <summary>
		/// cannot send after transport endpoint shutdown
		/// </summary>
		ESHUTDOWN,
		/// <summary>
		/// invalid seek
		/// </summary>
		ESPIPE,
		/// <summary>
		/// no such process
		/// </summary>
		ESRCH,
		/// <summary>
		/// connection timed out
		/// </summary>
		ETIMEDOUT,
		/// <summary>
		/// text file is busy
		/// </summary>
		ETXTBSY,
		/// <summary>
		/// cross-device link not permitted
		/// </summary>
		EXDEV,
		/// <summary>
		/// unknown error
		/// </summary>
		UNKNOWN,
		/// <summary>
		/// end of file
		/// </summary>
		EOF,
		/// <summary>
		/// no such device or address
		/// </summary>
		ENXIO,
		/// <summary>
		/// too many links
		/// </summary>
		EMLINK,
	}
}
