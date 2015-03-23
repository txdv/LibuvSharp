using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe public class UVException : Exception
	{
		/// <summary>
		/// Independent error code, has the same value on all
		/// systems.
		/// </summary>
		/// <value>The error code.</value>
		internal uv_err_code ErrorCode { get; set; }

		/// <summary>
		/// Gets the the underlying system error code of the error
		/// They might be different on windows and unix, EAGAIN
		/// for example is -4088 on windows while it is -11 on UNIX.
		/// </summary>
		/// <value>The system error code.</value>
		public int SystemErrorCode { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }

		public UVException(int systemErrorCode, string name, string description)
			: base(string.Format("{0}({1}): {2}", name, systemErrorCode, description))
		{
			ErrorCode = (uv_err_code)Enum.Parse(typeof(uv_err_code), "UV_" + name);
			SystemErrorCode = systemErrorCode;
			Name = name;
			Description = description;
		}

		internal UVException(int systemErrorCode)
			: this(systemErrorCode, ErrorName(systemErrorCode), StringError(systemErrorCode))
		{
		}


		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_strerror(int error);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *uv_err_name(int error);

		internal static string StringError(int error)
		{
			return new string(uv_strerror(error));
		}

		internal static string StringError(uv_err_code error)
		{
			return StringError((int)error);
		}

		internal static string ErrorName(int error)
		{
			return new string(uv_err_name(error));
		}
	}
}

