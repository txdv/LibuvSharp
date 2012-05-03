using System;

namespace LibuvSharp
{
	internal enum UvHandleType : int
	{
		UV_UNKNOWN_HANDLE = 0,
		UV_ARES_TASK,
		UV_ASYNC,
		UV_CHECK,
		UV_FS_EVENT,
		UV_IDLE,
		UV_NAMED_PIPE,
		UV_PREPARE,
		UV_PROCESS,
		UV_TCP,
		UV_TIMER,
		UV_TTY,
		UV_UDP,
		UV_HANDLE_TYPE_PRIVATE,
		UV_HANDLE_TYPE_MAX,
	}
}
