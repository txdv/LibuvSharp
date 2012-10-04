using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_connect_t
	{
		public RequestType type;
		public IntPtr data;
		/*
		#if !__MonoCS__
		NativeOverlapped overlapped;
		IntPtr queued_bytes;
		uv_err_t error;
		IntPtr next_req;
		#endif
		*/
		public IntPtr cb;
		public IntPtr handle;
	}

	unsafe internal class ConnectRequest : CallbackPermaRequest
	{
		uv_connect_t *connect;

		public ConnectRequest()
			: base(RequestType.UV_CONNECT)
		{
			connect = (uv_connect_t *)Handle;
		}

		public IntPtr ConnectHandle {
			get {
				return connect->handle;
			}
		}
	}
}

