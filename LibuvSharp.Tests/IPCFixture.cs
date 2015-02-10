using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using LibuvSharp.Threading;
using LibuvSharp.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace LibuvSharp.Tests
{
	public class IPCFixture
	{
		[Fact]
		public void CanSendHandles()
		{
			int count = 0;

			Loop.Default.Run(async () => {
				var handles = new Stack<Handle>();
				string name = "test";
				var pipelistener = new IPCPipeListener();
				pipelistener.Bind(name);
				pipelistener.Connection += () => {
					var client = pipelistener.Accept();
					client.Resume();
					client.HandleData += (handle, data) => {
						handles.Push(handle);
						count++;
						if (count == 3) {
							foreach (var h in handles) {
								h.Close();
							}
							pipelistener.Close();
						}
					};
				};
				pipelistener.Listen();

				var pipe = new IPCPipe();
				await pipe.ConnectAsync(name);


				var ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
				var tcplistener = new TcpListener();
				tcplistener.Bind(ipep);
				tcplistener.Connection += () => {
					var client = tcplistener.Accept();
					pipe.Write(client, new byte[1], (ex) => {
						client.Close();
						tcplistener.Close();
					});
				};
				tcplistener.Listen();

				var tcp = new Tcp();
				await tcp.ConnectAsync(ipep);
				tcp.Write("HELLO WORLD");

				var udp = new Udp();
				udp.Bind(ipep);
				pipe.Write(udp, Encoding.Default.GetBytes("UDP"), (ex) => udp.Close());
				pipe.Write(pipe, Encoding.Default.GetBytes("pipe"), (ex) => pipe.Close());
			});

			Assert.Equal(3, count);
		}
	}
}

