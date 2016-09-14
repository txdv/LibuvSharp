using System;
using System.Net;
using System.Threading.Tasks;

namespace LibuvSharp.Threading.Tasks
{
	public static class IConnectableExtensions
	{
		public static Task ConnectAsync<TType, TEndPoint>(this IConnectable<TType, TEndPoint> client, TEndPoint endPoint)
		{
			return HelperFunctions.Wrap(endPoint, client.Connect);
		}

		public static Task ConnectAsync<TType>(this IConnectable<TType, IPEndPoint> client, IPAddress ipAddress, int port)
		{
			return HelperFunctions.Wrap(ipAddress, port, client.Connect);
		}

		public static Task ConnectAsync<TType>(this IConnectable<TType, IPEndPoint> client, string ipAddress, int port)
		{
			return HelperFunctions.Wrap(ipAddress, port, client.Connect);
		}

		public static Task ConnectAsync<TType, TEndPoint>(this IConnectable<TType, TEndPoint> client, ILocalAddress<TEndPoint> localAddress)
		{
			return HelperFunctions.Wrap(localAddress, client.Connect);
		}
	}
}

