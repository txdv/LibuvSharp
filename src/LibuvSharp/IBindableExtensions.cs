using System;
using System.Net;

namespace LibuvSharp
{
	public static class IBindableExtensions
	{
		public static void Bind<T>(this IBindable<T, IPEndPoint> bindable, IPAddress ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			bindable.Bind(new IPEndPoint(ipAddress, port));
		}

		public static void Bind<T>(this IBindable<T, IPEndPoint> bindable, string ipAddress, int port)
		{
			Ensure.ArgumentNotNull(ipAddress, "ipAddress");
			bindable.Bind(IPAddress.Parse(ipAddress), port);
		}
	}
}

