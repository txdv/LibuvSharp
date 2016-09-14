using System;

namespace LibuvSharp
{
	unsafe class WorkRequest : PermaRequest
	{
		public static readonly int Size = UV.Sizeof(LibuvSharp.RequestType.UV_WORK);

		public WorkRequest()
			: base(Size)
		{
		}

		Action before;
		Action after;

		public WorkRequest(Action before, Action after)
			: this()
		{
			this.before = before;
			this.after = after;
		}

		public static void BeforeCallback(IntPtr req)
		{
			var workreq = PermaRequest.GetObject<WorkRequest>(req);
			workreq.before();
		}

		public static void AfterCallback(IntPtr req)
		{
			var workreq = PermaRequest.GetObject<WorkRequest>(req);
			workreq.after();
			workreq.Dispose();
		}
	}
}

