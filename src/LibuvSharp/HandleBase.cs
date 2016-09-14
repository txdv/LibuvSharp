using System;

namespace LibuvSharp
{
	public abstract partial class HandleBase : Handle
	{
		internal HandleBase(Loop loop, IntPtr handle)
			: base(loop, handle)
		{
		}

		internal HandleBase(Loop loop, int size)
			: this(loop, UV.Alloc(size))
		{
		}

		internal HandleBase(Loop loop, HandleType type)
			: this(loop, Handle.Size(type))
		{
		}

		internal HandleBase(Loop loop, HandleType type, Func<IntPtr, IntPtr, int> constructor)
			: this(loop, type)
		{
			Construct(constructor);
		}
	}
}

