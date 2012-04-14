using System;

namespace Libuv
{
	public class UVException : Exception
	{
		public string Description { get; protected set; }
		public int Code { get; protected set; }
		public string Name { get; protected set; }

		internal UVException(Loop loop)
			: this(UV.uv_last_error(loop.Handle))
		{
		}

		internal UVException(uv_err_t error)
			: base(string.Format("{0}({1}): {2}", error.Name, error.code, error.Description))
		{
			Description = error.Description;
			Code = (int)error.code;
			Name = error.Name;
		}
	}
}

