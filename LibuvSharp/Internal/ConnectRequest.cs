using System;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct uv_connect_t
	{
		public UvRequestType type;
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
			: base(UvRequestType.UV_CONNECT)
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

