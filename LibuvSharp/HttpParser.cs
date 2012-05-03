using System;
using System.Text;
using System.Runtime.InteropServices;

namespace LibuvSharp
{
	[StructLayout(LayoutKind.Sequential)]
	struct http_parser
	{
		byte typeFlags;
		byte state;
		byte header_state;
		byte index;

		uint nread;
		ulong content_length;

		// read only
		public readonly short http_major;
		public readonly short http_minor;
		public readonly short status_code;
		public readonly byte method;
		public readonly byte errorUpgrade;

		public IntPtr data;

		public bool Upgrade {
			get {
				return (errorUpgrade & 128) == 128;
			}
		}

		public http_errno Error {
			get {
				return (http_errno)(errorUpgrade & 127);
			}
		}
	}

	/*
	struct http_parser_settings_clr
	{
		public Func<IntPtr, int>            on_message_begin;
		public Func<IntPtr, IntPtr, IntPtr> on_url;
		public Func<IntPtr, IntPtr, IntPtr> on_header_field;
		public Func<IntPtr, IntPtr, IntPtr> on_header_value;
		public Func<IntPtr, int>            on_header_complete;
		public Func<IntPtr, IntPtr, IntPtr> on_body;
		public Func<IntPtr>                 on_message_complete;
	}
	*/

	[StructLayout(LayoutKind.Sequential)]
	struct http_parser_settings
	{
		public IntPtr on_message_begin;
		public IntPtr on_url;
		public IntPtr on_header_field;
		public IntPtr on_header_value;
		public IntPtr on_headers_complete;
		public IntPtr on_body;
		public IntPtr on_message_complete;
	}

	public enum http_method
	{
		HTTP_DELETE,
		HTTP_GET,
		HTTP_HEAD,
		HTTP_POST,
		HTTP_PUT,
		HTTP_CONNECT,
		HTTP_OPTIONS,
		HTTP_TRACE,
		HTTP_COPY,
		HTTP_LOCK,
		HTTP_MKCOL,
		HTTP_MOVE,
		HTTP_PROPFIND,
		HTTP_PROPATCH,
		HTTP_UNLOCK,
		HTTP_REPORT,
		HTTP_MKACTIVITY,
		HTTP_CHECKOUT,
		HTTP_MERGE,
		HTTP_MSEARCH,
		HTTP_NOTIFY,
		HTTP_SUSCRIBE,
		HTTP_UNSUBSCRIBE,
		HTTP_PATCH,
	}

	public enum http_parser_type
	{
		HTTP_REQUEST,
		HTTP_RESPONSE,
		HTTP_BOTH
	}

	public enum http_errno
	{
		HPE_OK,

		HPE_CB_message_begin,
		HPE_CB_path,
		HPE_CB_query_string,
		HPE_CB_url,
		HPE_CB_fragment,
		HPE_CB_header_field,
		HPE_CB_header_value,
		HPE_CB_headers_complete,
		HPE_CB_body,
		HPE_CB_message_complete,

		HPE_INVALID_EOF_STATE,
		HPE_HEADER_OVERFLOW,
		HPE_CLOSED_CONNECTION,
		HPE_INVALID_VERSION,
		HPE_INVALID_STATUS,
		HPE_INVALID_METHOD,
		HPE_INVALID_URL,
		HPE_INVALID_HOST,
		HPE_INVALID_PORT,
		HPE_INVALID_PATH,
		HPE_INVALID_QUERY_STRING,
		HPE_INVALID_FRAGMENT,
		HPE_LF_EXPECTED,
		HPE_INVALID_HEADER_TOKEN,
		HPE_INVALID_CONTENT_LENGTH,
		HPE_INVALID_CHUNK_SIZE,
		HPE_INVALID_CONSTANT,
		HPE_INVALID_INTERNAL_STATE,
		HPE_STRICT,
		HPE_UNKNOWN
	}

	unsafe public class HttpParser : IDisposable
	{
		http_parser *parser;
		http_parser_settings settings;

		Func<IntPtr, int>                 onMessageBegin;
		Func<IntPtr, IntPtr, IntPtr, int> onUrl;
		Func<IntPtr, IntPtr, IntPtr, int> onHeaderField;
		Func<IntPtr, IntPtr, IntPtr, int> onHeaderValue;
		Func<IntPtr, int>                 onHeadersComplete;
		Func<IntPtr, IntPtr, IntPtr, int> onBody;
		Func<IntPtr, int>                 onMessageComplete;

		BufferPin buffer;

		public HttpParser()
			: this(http_parser_type.HTTP_BOTH)
		{
		}

		public HttpParser(http_parser_type type)
		{
			ParserPointer = Marshal.AllocHGlobal(sizeof(http_parser));
			http_parser_init(ParserPointer, type);

			onMessageBegin    = OnMessageBegin;
			onUrl             = OnUrl;
			onHeaderField     = OnHeaderField;
			onHeaderValue     = OnHeaderValue;
			onHeadersComplete = OnHeadersComplete;
			onBody            = OnBody;
			onMessageComplete = OnMessageComplete;

			settings.on_message_begin    = Marshal.GetFunctionPointerForDelegate(onMessageBegin);
			settings.on_url              = Marshal.GetFunctionPointerForDelegate(onUrl);
			settings.on_header_field     = Marshal.GetFunctionPointerForDelegate(onHeaderField);
			settings.on_header_value     = Marshal.GetFunctionPointerForDelegate(onHeaderValue);
			settings.on_headers_complete = Marshal.GetFunctionPointerForDelegate(onHeadersComplete);
			settings.on_body             = Marshal.GetFunctionPointerForDelegate(onBody);
			settings.on_message_complete = Marshal.GetFunctionPointerForDelegate(onMessageComplete);
		}

		~HttpParser()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void Dispose(bool disposing)
		{
			if (ParserPointer != IntPtr.Zero) {
				Marshal.FreeHGlobal(ParserPointer);
			}
			ParserPointer = IntPtr.Zero;
		}

		int OnMessageBegin(IntPtr ptr)
		{
			return OnMessageBegin();
		}
		int OnUrl(IntPtr ptr, IntPtr at, IntPtr length)
		{
			return OnUrl(buffer.Buffer, buffer.GetOffset(at).ToInt32(), (int)length);
		}
		int OnHeaderField(IntPtr ptr, IntPtr at, IntPtr length)
		{
			return OnHeaderField(buffer.Buffer, buffer.GetOffset(at).ToInt32(), (int)length);
		}
		int OnHeaderValue(IntPtr ptr, IntPtr at, IntPtr length)
		{
			return OnHeaderValue(buffer.Buffer, buffer.GetOffset(at).ToInt32(), (int)length);
		}
		int OnHeadersComplete(IntPtr ptr)
		{
			return OnHeadersComplete();
		}
		int OnBody(IntPtr ptr, IntPtr at, IntPtr length)
		{
			return OnBody(buffer.Buffer, buffer.GetOffset(at).ToInt32(), (int)length);
		}
		int OnMessageComplete(IntPtr ptr)
		{
			return OnMessageComplete();
		}

		public virtual int OnMessageBegin()
		{
			return 0;
		}
		public virtual int OnUrl(byte[] data, int start, int count)
		{
			return 0;
		}
		public virtual int OnHeaderField(byte[] data, int start, int count)
		{
			return 0;
		}
		public virtual int OnHeaderValue(byte[] data, int start, int count)
		{
			return 0;
		}
		public virtual int OnHeadersComplete()
		{
			return 0;
		}
		public virtual int OnBody(byte[] data, int start, int count)
		{
			return 0;
		}
		public virtual int OnMessageComplete()
		{
			return 0;
		}

		IntPtr ParserPointer {
			get {
				return (IntPtr)parser;
			}
			set {
				parser = (http_parser *)value;
			}
		}

		public int HttpMajor {
			get {
				return parser->http_major;
			}
		}

		public int HttpMinor {
			get {
				return parser->http_minor;
			}
		}

		public int StatusCode {
			get {
				return parser->status_code;
			}
		}

		public http_method Method {
			get {
				return (http_method)parser->method;
			}
		}

		public bool Upgrade {
			get {
				return parser->Upgrade;
			}
		}

		public http_errno Errno {
			get {
				return parser->Error;
			}
		}

		public string ErrorName {
			get {
				return new string(http_errno_name(Errno));
			}
		}

		public string ErrorDescription {
			get {
				return new string(http_errno_description(Errno));
			}
		}

		public void YieldException()
		{
			if (Errno != http_errno.HPE_OK) {
				// TODO: create exceptions for every error
				throw new Exception(string.Format("{0}: {1}", ErrorName, ErrorDescription));
			}
		}

		public void Execute(byte[] data, int length)
		{
			using (buffer = new BufferPin(data))
			{
				var handle = GCHandle.Alloc(settings, GCHandleType.Pinned);
				http_parser_execute(ParserPointer, handle.AddrOfPinnedObject(), buffer.Pointer, (IntPtr)length);
				handle.Free();
			}
			buffer = null;
		}

		public void Execute(byte[] data)
		{
			Execute(data, data.Length);
		}

		public void Execute(Encoding enc, string str)
		{
			byte[] data = enc.GetBytes(str);
			Execute(data);
		}

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern void http_parser_init(IntPtr parser, http_parser_type type);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr http_parser_execute(IntPtr parser, IntPtr settings, IntPtr data, IntPtr length);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern int http_should_keep_alive(IntPtr parser);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *http_method_str(http_method m);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *http_errno_name(http_errno err);

		[DllImport("uv", CallingConvention = CallingConvention.Cdecl)]
		private static extern sbyte *http_errno_description(http_errno err);
	}

	public class EventedHttpParser : HttpParser
	{
		public EventedHttpParser()
			: base()
		{
		}

		public EventedHttpParser(http_parser_type type)
			: base(type)
		{
		}

		public event Action OnMessageBeginEvent;
		public override int OnMessageBegin()
		{
			if (OnMessageBeginEvent != null) {
				OnMessageBeginEvent();	
			}
			
			return base.OnMessageBegin();
		}

		public event Action<byte[], int, int> OnUrlEvent;
		public override int OnUrl(byte[] data, int start, int count)
		{
			if (OnUrlEvent != null) {
				OnUrlEvent(data, start, count);
			}
			return base.OnUrl (data, start, count);
		}

		public event Action<byte[], int, int> OnHeaderFieldEvent;
		public override int OnHeaderField(byte[] data, int start, int count)
		{
			if (OnHeaderFieldEvent != null) {
				OnHeaderFieldEvent(data, start, count);
			}

			return base.OnHeaderField(data, start, count);
		}

		public event Action<byte[], int, int> OnHeaderValueEvent;
		public override int OnHeaderValue(byte[] data, int start, int count)
		{
			if (OnHeaderValueEvent != null) {
				OnHeaderValueEvent(data, start, count);
			}

			return base.OnHeaderValue(data, start, count);
		}

		public event Action OnHeadersCompleteEvent;
		public override int OnHeadersComplete()
		{
			if (OnHeadersCompleteEvent != null) {
				OnHeadersCompleteEvent();
			}

			return base.OnHeadersComplete ();
		}

		public event Action<byte[], int, int> OnBodyEvent;
		public override int OnBody(byte[] data, int start, int count)
		{
			if (OnBodyEvent != null) {
				OnBodyEvent(data, start, count);
			}
			return base.OnBody(data, start, count);
		}

		public event Action OnMessageCompleteEvent;
		public override int OnMessageComplete()
		{
			if (OnMessageCompleteEvent != null) {
				OnMessageCompleteEvent();
			}

			return base.OnMessageComplete();
		}
	}

	public class EncodedHttpParser : EventedHttpParser
	{
		public Encoding Encoding { get; protected set; }

		public EncodedHttpParser()
			: this(Encoding.Default)
		{
		}

		public EncodedHttpParser(Encoding enc)
			: this(http_parser_type.HTTP_BOTH, enc)
		{
		}

		public EncodedHttpParser(http_parser_type type)
			: this(type, Encoding.Default)
		{
		}

		public EncodedHttpParser(http_parser_type type, Encoding enc)
			: base(type)
		{
			Encoding = enc;
		}

		public event Action<string, string> OnHeaderElementEvent;

		string field = null;
		public override int OnHeaderField(byte[] data, int start, int count)
		{
			field = Encoding.GetString(data, start, count);

			return base.OnHeaderField(data, start, count);
		}

		public override int OnHeaderValue(byte[] data, int start, int count)
		{
			if (OnHeaderElementEvent != null) {
				OnHeaderElementEvent(field, Encoding.GetString(data, start, count));
			}

			return base.OnHeaderValue (data, start, count);
		}
	}
}

