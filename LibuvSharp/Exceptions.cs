using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe public class UVException : Exception
	{
		public int Code { get; protected set; }
		public string Name { get; protected set; }
		public string Description { get; protected set; }

		public UVException(int code, string name, string description)
			: base(string.Format("{0}({1}): {2}", name, code, description))
		{
			Code = code;
			Name = name;
			Description = description;
		}

		internal UVException(int errorCode)
			: this(errorCode, StringError(errorCode), ErrorName(errorCode))
		{
		}

		internal UVException(uv_err_code errorCode)
			: this((int)errorCode)
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

