using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	unsafe internal struct uv_err_t
	{
		public uv_err_t(int error)
		{
			code = (uv_err_code)error;
			sys_errno_ = 0;
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_strerror(uv_err_t error);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_err_name(uv_err_t error);

		public uv_err_code code;
		#pragma warning disable 414
		int sys_errno_;
		#pragma warning restore 414

		public string Description {
			get {
				return new string(uv_strerror(this));
			}
		}

		public string Name {
			get {
				return new string(uv_err_name(this));
			}
		}
	}
}
