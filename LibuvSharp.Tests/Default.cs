using System;
using System.Net;

namespace LibuvSharp.Tests
{
	public class IPInfo
	{
		public IPInfo(string str, int port)
		{
			IPAddressString = str;
			IPAddress = IPAddress.Parse(IPAddressString);
			Port = port;
			IPEndPoint = new IPEndPoint(IPAddress, Port);
		}

		public string IPAddressString { get; private set; }
		public IPAddress IPAddress { get; private set; }
		public int Port { get; private set; }
		public IPEndPoint IPEndPoint { get; private set; }
	}

	public static class Default
	{
		static Default()
		{
			Port = 8000;
			IPv4 = new IPInfo("127.0.0.1", Port);
			IPv6 = new IPInfo("::1", Port);

			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				Pipename = "testpipe";
			} else {
				Pipename = @"\\.\pipe\testpipe";
			}
		}

		public static int Port { get; private set; }
		public static IPInfo IPv4 { get; private set; }
		public static IPInfo IPv6 { get; private set; }

		public static string Pipename { get; private set; }
	}
}

