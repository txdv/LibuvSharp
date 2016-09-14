using System;
using System.Threading.Tasks;
using System.Reflection;

namespace LibuvSharp
{
	class HelperFunctions
	{
		#if TASK_STATUS
		static System.Reflection.FieldInfo monoStatusField;
		static System.Reflection.FieldInfo dotNetStatusField;
		// http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs,189
		internal const int TASK_STATE_DELEGATE_INVOKED = 0x20000;

		static HelperFunctions()
		{
			monoStatusField = typeof(Task).GetField("status", BindingFlags.NonPublic | BindingFlags.Instance);
			dotNetStatusField = typeof(Task).GetField("m_stateFlags", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public static void SetStatus(Task task, TaskStatus status)
		{
			if (monoStatusField != null) {
				monoStatusField.SetValue(task, status);
			} else if (dotNetStatusField != null) {
				if (status != TaskStatus.Running) {
					throw new ApplicationException("SetStatus only supported with status = TaskStatus.Running");
				}
				dotNetStatusField.SetValue(task, (int)dotNetStatusField.GetValue(task) | TASK_STATE_DELEGATE_INVOKED);
			} else {
				throw new ApplicationException("Platform not supported: The Status of the Task can't be set to Running.");
			}
		}
		#endif

		public static Action<Exception, T> Finish<T>(TaskCompletionSource<T> tcs, Action callback)
		{
			bool finished = false;

			return (Exception exception, T value) => {
				if (finished) {
					return;
				}

				finished = true;

				if (callback != null) {
					callback();
				}

				if (exception != null) {
					tcs.SetException(exception);
				} else {
					tcs.SetResult(value);
				}
			};
		}

		public static Action<Exception> Exception(TaskCompletionSource<object> tcs)
		{
			return (Exception exception) => {
				if (exception != null) {
					tcs.SetException(exception);
				} else {
					tcs.SetResult(null);
				}
			};
		}

		public static Action<Exception, TResult> Exception<TResult>(TaskCompletionSource<TResult> tcs)
		{
			return (Exception exception, TResult result) => {
				if (exception != null) {
					tcs.SetException(exception);
				} else {
					tcs.SetResult(result);
				}
			};
		}

		public static Task WrapSingle(Action<Action> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(() => tcs.SetResult(null));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task Wrap(Action<Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task Wrap<T>(T arg1, Action<T, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task Wrap<T1, T2>(T1 arg1, T2 arg2, Action<T1, T2, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, arg2, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task Wrap<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, Action<T1, T2, T3, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, arg2, arg3, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task Wrap<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, Action<T1, T2, T3, T4, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, arg2, arg3, arg4, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task Wrap<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, Action<T1, T2, T3, T4, T5, Action<Exception>> action)
		{
			var tcs = new TaskCompletionSource<object>();
			try {
				action(arg1, arg2, arg3, arg4, arg5, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task<TResult> Wrap<T, TResult>(T arg1, Func<T, Action<Exception>, TResult> func)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				var res = default(TResult);
				res = func(arg1, (ex) => {
					if (ex == null) {
						tcs.SetResult(res);
					} else {
						tcs.SetException(ex);
					}
				});
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task<TResult> Wrap<T1, T2, TResult>(T1 arg1, T2 arg2, Func<T1, T2, Action<Exception>, TResult> func)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				var res = default(TResult);
				res = func(arg1, arg2, (ex) => {
					if (ex == null) {
						tcs.SetResult(res);
					} else {
						tcs.SetException(ex);
					}
				});
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task<TResult> Wrap<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3, Func<T1, T2, T3, Action<Exception>, TResult> func)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				var res = default(TResult);
				res = func(arg1, arg2, arg3, (ex) => {
					if (ex == null) {
						tcs.SetResult(res);
					} else {
						tcs.SetException(ex);
					}
				});
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}

		public static Task<TResult> Wrap<T, TResult>(T arg1, Action<T, Action<Exception, TResult>> action)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				action(arg1, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task<TResult> Wrap<T1, T2, TResult>(T1 arg1, T2 arg2, Action<T1, T2, Action<Exception, TResult>> action)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				action(arg1, arg2, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task<TResult> Wrap<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3, Action<T1, T2, T3, Action<Exception, TResult>> action)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				action(arg1, arg2, arg3, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task<TResult> Wrap<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, Action<T1, T2, T3, T4, Action<Exception, TResult>> action)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				action(arg1, arg2, arg3, arg4, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
		public static Task<TResult> Wrap<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, Action<T1, T2, T3, T4, T5, Action<Exception, TResult>> action)
		{
			var tcs = new TaskCompletionSource<TResult>();
			try {
				action(arg1, arg2, arg3, arg4, arg5, Exception(tcs));
				#if TASK_STATUS
				SetStatus(tcs.Task, TaskStatus.Running);
				#endif
			} catch (Exception ex) {
				tcs.SetException(ex);
			}
			return tcs.Task;
		}
	}
}

