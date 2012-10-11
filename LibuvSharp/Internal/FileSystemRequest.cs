using System;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	unsafe internal class FileSystemRequest : PermaRequest
	{
		private static readonly int Size = UV.Sizeof(RequestType.UV_FS);

		protected uv_fs_t *fsrequest;

		public FileSystemRequest()
			: base(Size)
		{
			fsrequest = (uv_fs_t *)Handle;
		}

		public Action<Exception, FileSystemRequest> Callback { get; set; }

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern void uv_fs_req_cleanup(IntPtr req);

		public override void Dispose(bool disposing)
		{
			uv_fs_req_cleanup(Handle);
			base.Dispose(disposing);
		}

		public IntPtr Result {
			get {
				return fsrequest->result;
			}
		}

		public int Error {
			get {
				return fsrequest->error;
			}
		}

		public IntPtr Pointer {
			get {
				return fsrequest->ptr;
			}
		}

		public void End(IntPtr ptr)
		{
			Exception e = null;
			if (Result.ToInt32() == -1) {
				uv_err_t err = new uv_err_t(Error);
				e = Ensure.Map(err);
			}

			if (Callback != null) {
				Callback(e, this);
			}
			Dispose();
		}

		public static void StaticEnd(IntPtr ptr)
		{
			PermaRequest.GetObject<FileSystemRequest>(ptr).End(ptr);
		}
	}
}

