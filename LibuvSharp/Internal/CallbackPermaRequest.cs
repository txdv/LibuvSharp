using System;

namespace LibuvSharp
{
	internal class CallbackPermaRequest : PermaRequest
	{
		public CallbackPermaRequest(int size)
			: this(size, true)
		{
		}

		public CallbackPermaRequest(int size, bool allocate)
			: base(size, allocate)
		{
		}

		public CallbackPermaRequest(UvRequestType type)
			: this(type, true)
		{
		}

		public CallbackPermaRequest(UvRequestType type, bool allocate)
			: this(UV.Sizeof(type), allocate)
		{
		}

		public Action<int, CallbackPermaRequest> Callback { get; set; }

		protected void End(IntPtr ptr, int status)
		{
			Callback(status, this);
			Dispose();
		}

		static public void StaticEnd(IntPtr ptr, int status)
		{
			var obj = PermaRequest.GetObject<CallbackPermaRequest>(ptr);
			if (obj == null) {
				throw new Exception("Target is null");
			} else {
				obj.End(ptr, status);
			}
		}
	}
}

