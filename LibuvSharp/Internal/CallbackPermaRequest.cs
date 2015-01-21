using System;

namespace LibuvSharp
{
	internal class CallbackPermaRequest : PermaRequest
	{
		public CallbackPermaRequest(int size)
			: base(size)
		{
		}

		public CallbackPermaRequest(RequestType type)
			: this(UV.Sizeof(type))
		{
		}

		public Action<int, CallbackPermaRequest> Callback { get; set; }

		protected void End(IntPtr ptr, int status)
		{
			Callback(status, this);
			Dispose();
		}

		public static Handle.callback CallbackDelegate = StaticEnd;

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

