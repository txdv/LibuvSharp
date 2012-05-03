using System;

namespace LibuvSharp
{
	internal enum UvRequestType : int
	{
		UV_UNKNOWN_REQ = 0,
		UV_CONNECT,
		UV_WRITE,
		UV_SHUTDOWN,
		UV_UDP_SEND,
		UV_FS,
		UV_WORK,
		UV_GETADDRINFO,
		UV_REQ_TYPE_PRIVATE,
		UV_REQ_TYPE_MAX,
	}
}
