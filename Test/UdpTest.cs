using System;
using System.Text;
using System.Net;
using Libuv;

namespace Test
{
	public class UdpTest
	{
		public static void Run()
		{
			Run(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000));
			Run(new IPEndPoint(IPAddress.Parse("::1"), 8000));
		}

		public static void Run(IPEndPoint ep)
		{
			int close_cb_called = 0;
			int cl_send_cb_called = 0;
			int cl_recv_cb_called = 0;
			int sv_send_cb_called = 0;
			int sv_recv_cb_called = 0;

			Udp client = new Udp();
			Udp server = new Udp();

			server.Bind(ep);
			server.Receive(Encoding.ASCII, (str, rinfo) => {
				Debug.Assert(str == "PING");
				sv_recv_cb_called++;
				server.Send(rinfo, Encoding.ASCII.GetBytes("PONG"), () => {
					sv_send_cb_called++;
					server.Close(() => { close_cb_called++; });
				});
			});

			client.Send(ep, Encoding.ASCII.GetBytes("PING"), () => {
				cl_send_cb_called++;
				client.Receive(Encoding.ASCII, (str, rinfo) => {
					Debug.Assert(str == "PONG");
					cl_recv_cb_called++;
					client.Close(() => { close_cb_called++; });
				});
			});


			Debug.Assert(close_cb_called == 0);
			Debug.Assert(cl_send_cb_called == 0);
			Debug.Assert(cl_recv_cb_called == 0);
			Debug.Assert(sv_send_cb_called == 0);
			Debug.Assert(sv_recv_cb_called == 0);

			Loop.Default.Run();

			Debug.Assert(close_cb_called == 2);
			Debug.Assert(cl_send_cb_called == 1);
			Debug.Assert(cl_recv_cb_called == 1);
			Debug.Assert(sv_send_cb_called == 1);
			Debug.Assert(sv_recv_cb_called == 1);

			Debug.Assert(UV.PointerCount == 0);
		}
	}
}

