using System;

namespace LibuvSharp
{
	enum RequestType : int
	{
		UV_UNKNOWN_REQ = 0,
		UV_REQ,
		UV_CONNECT,
		UV_WRITE,
		UV_SHUTDOWN,
		UV_UDP_SEND,
		UV_FS,
		UV_WORK,
		UV_GETADDRINFO,
		UV_GETNAMEINFO,
		UV_REQ_TYPE_PRIVATE,
		UV_REQ_TYPE_MAX,
	}
}
