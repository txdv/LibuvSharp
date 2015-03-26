using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	public partial class HandleBase : ISendBufferSize, IReceiveBufferSize
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int buffer_size_function(IntPtr handle, out int value);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_send_buffer_size(IntPtr handle, out int value);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		static extern int uv_recv_buffer_size(IntPtr handle, out int value);

		int Invoke(buffer_size_function function, int value)
		{
			CheckDisposed();

			int r = function(NativeHandle, out value);
			Ensure.Success(r);
			return r;
		}

		int Apply(buffer_size_function buffer_size, int value)
		{
			return Invoke(buffer_size, value);
		}

		public int SendBufferSize {
			get {
				return Apply(uv_send_buffer_size, 0);
			}
			set {
				Apply(uv_send_buffer_size, @value);
			}
		}

		public int ReceiveBufferSize {
			get {
				return Apply(uv_recv_buffer_size, 0);
			}
			set {
				Apply(uv_recv_buffer_size, @value);
			}
		}
	}
}

