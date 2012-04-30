using System;
using System.Runtime.InteropServices;

namespace Libuv
{
	public class DynamicLibrary
	{
		[DllImport("uv")]
		internal extern static uv_err_t uv_dlopen(IntPtr handle, out IntPtr ptr);

		[DllImport("uv")]
		internal extern static uv_err_t uv_dlclose(IntPtr handle);

		[DllImport("uv")]
		internal extern static uv_err_t uv_dlsym(IntPtr handle, string name, out IntPtr ptr);

		IntPtr handle;

		public bool Closed {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public DynamicLibrary()
		{
			Ensure.Success(uv_dlopen(IntPtr.Zero, out handle));
		}

		public DynamicLibrary(string str)
		{
			var ptr = Marshal.StringToHGlobalAnsi(str);
			var error = uv_dlopen(ptr, out handle);
			Marshal.FreeHGlobal(ptr);
			Ensure.Success(error);
		}

		void Close()
		{
			if (!Closed) {
				uv_dlclose(handle);
				handle = IntPtr.Zero;
			}
		}

		public bool TryGetSymbol(string name, out IntPtr pointer)
		{
			return uv_dlsym(handle, name, out pointer).code == uv_err_code.UV_OK;
		}

		public IntPtr GetSymbol(string name)
		{
			IntPtr ptr;
			Ensure.Success(uv_dlsym(handle, name, out ptr));
			return ptr;
		}
	}
}

