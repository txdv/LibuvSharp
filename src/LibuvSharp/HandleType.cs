using System;

namespace LibuvSharp
{
	public enum HandleType : int
	{
		UV_UNKNOWN_HANDLE = 0,
		UV_ASYNC,
		UV_CHECK,
		UV_FS_EVENT,
		UV_FS_POLL,
		UV_HANDLE,
		UV_IDLE,
		UV_NAMED_PIPE,
		UV_POLL,
		UV_PREPARE,
		UV_PROCESS,
		UV_STREAM,
		UV_TCP,
		UV_TIMER,
		UV_TTY,
		UV_UDP,
		UV_SIGNAL,
		UV_FILE,
		UV_HANDLE_TYPE_PRIVATE,
		UV_HANDLE_TYPE_MAX,
	}
}
