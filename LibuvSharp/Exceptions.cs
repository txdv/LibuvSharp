using System;

namespace LibuvSharp
{
	public class UVException : Exception
	{
		public string Description { get; protected set; }
		public int Code { get; protected set; }
		public string Name { get; protected set; }

		public UVException(int code, string name, string description)
			: base(string.Format("{0}({1}): {2}", name, code, description))
		{
			Code = code;
			Name = name;
			Description = description;
		}

		internal UVException(Loop loop)
			: this(Ensure.uv_last_error(loop.NativeHandle))
		{
		}

		internal UVException(uv_err_t error)
			: this((int)error.code, error.Name, error.Description)
		{
		}
	}
}

