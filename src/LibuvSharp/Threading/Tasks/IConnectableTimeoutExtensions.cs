using System;
using System.Net;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IConnectableTimeoutExtensions
	{
		public static Task ConnectAsync<TType, TEndPoint>(this IConnectable<TType, TEndPoint> client, TEndPoint endPoint, TimeSpan timeout)
		{
			return HelperFunctions.Wrap(endPoint, Timeout.In<TEndPoint>(timeout, client.Connect));
		}

		public static Task ConnectAsync<TType>(this IConnectable<TType, IPEndPoint> client, IPAddress address, int port, TimeSpan timeout)
		{
			return HelperFunctions.Wrap(address, port, Timeout.In<IPAddress, int>(timeout, client.Connect));
		}

		public static Task ConnectAsync<TType>(this IConnectable<TType, IPEndPoint> client, string address, int port, TimeSpan timeout)
		{
			return HelperFunctions.Wrap(address, port, Timeout.In<string, int>(timeout, client.Connect));
		}
	}
}

