using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	class CActionBase : IDisposable
	{
		GCHandle GCHandle { get; set; }

		public CActionBase()
		{
			GCHandle = GCHandle.Alloc(this);
		}
		~CActionBase()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (GCHandle.IsAllocated) {
				GCHandle.Free();
			}
		}
	}

	class CAction : CActionBase
	{
		public Action Callback { get; protected set; }

		Action cb;

		public CAction(Action callback)
			: base()
		{
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback()
		{
			if (cb != null) {
				cb();
			}

			Dispose();
		}
	}

	class CAction<T1> : CActionBase
	{
		public Action<T1> Callback { get; protected set; }

		Action<T1> cb;

		public CAction(Action<T1> callback)
			: base()
		{
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T1 arg1)
		{
			if (cb != null) {
				cb(arg1);
			}

			Dispose();
		}
	}

	class CAction<T1, T2> : CActionBase
	{
		public Action<T1, T2> Callback { get; protected set; }

		Action<T1, T2> cb;

		public CAction(Action<T1, T2> callback)
			: base()
		{
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T1 arg1, T2 arg2)
		{
			if (cb != null) {
				cb(arg1, arg2);
			}

			Dispose();
		}
	}

	class CAction<T1, T2, T3> : CActionBase
	{
		public Action<T1, T2, T3> Callback { get; protected set; }

		Action<T1, T2, T3> cb;

		public CAction(Action<T1, T2, T3> callback)
		{
			cb = callback;
			Callback = PrivateCallback;
		}

		void PrivateCallback(T1 arg1, T2 arg2, T3 arg3)
		{
			if (cb != null) {
				cb(arg1, arg2, arg3);
			}

			Dispose();
		}
	}
}

