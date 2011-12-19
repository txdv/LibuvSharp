using System;
using Libuv;
using System.Net;
using System.Text;

namespace Test
{
	public class TcpTest
	{
		public static void Run()
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			Tcp server = new Tcp();
			server.Bind("127.0.0.1", 8000);
			server.Listen(128, (stream) => {
				stream.Start();
				stream.Read(Encoding.ASCII, (str) => {
					sv_recv_cb_called++;
					Debug.Assert(str == "PING");
					stream.Write(Encoding.ASCII, "PONG", () => { sv_send_cb_called++; });

					stream.Close(() => { close_cb_called++; });
					server.Close(() => { close_cb_called++; });
				});
			});

			Tcp client = new Tcp();
			client.Connect("127.0.0.1", 8000, (stream) => {
				stream.Start();
				stream.Write(Encoding.ASCII, "PING", () => { cl_send_cb_called++; });
				stream.Read(Encoding.ASCII, (str) => {
					cl_recv_cb_called++;
					Debug.Assert(str == "PONG");
					stream.Close(() => { close_cb_called++; });
					//client.Close(() => { close_cb_called++; });
				});
			});


			Debug.Assert(close_cb_called == 0);
			Debug.Assert(cl_send_cb_called == 0);
			Debug.Assert(cl_recv_cb_called == 0);
			Debug.Assert(sv_send_cb_called == 0);
			Debug.Assert(sv_recv_cb_called == 0);

			Loop.Default.Run();

			Debug.Assert(close_cb_called == 3);
			Debug.Assert(cl_send_cb_called == 1);
			Debug.Assert(cl_recv_cb_called == 1);
			Debug.Assert(sv_send_cb_called == 1);
			Debug.Assert(sv_recv_cb_called == 1);
#if DEBUG
			//Debug.Assert(UV.PointerCount == 0);
#endif
		}
	}
}

