using System;
using System.Net;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class TcpExtensions
	{
		public static Task ConnectAsync(this Tcp tcp, IPEndPoint ep)
		{
			return HelperFunctions.Wrap(ep, tcp.Connect);
		}

		public static Task ConnectAsync(this Tcp tcp, IPAddress ipAddress, int port)
		{
			return HelperFunctions.Wrap(ipAddress, port, tcp.Connect);
		}

		public static Task ConnectAsync(this Tcp tcp, string ipAddress, int port)
		{
			return HelperFunctions.Wrap(ipAddress, port, tcp.Connect);
		}
	}
}

