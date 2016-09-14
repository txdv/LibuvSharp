using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LibuvSharp;
using LibuvSharp.Threading.Tasks;

namespace Examples
{
    public static class Program
    {
        const string response = @"HTTP/1.1 200 OK
Server: lol
Content-Type: text/plain
Content-Length: 10
Connection: Close

HelloWorld";
        public static void Main(string[] args)
        {
            Loop.Default.Run(async () =>
            {
                Console.WriteLine("Starting example.");
                await Task.WhenAll(Server(), Client());
                Console.WriteLine("All finished.");
            });
        }

        public static async Task Server()
        {
            try
            {
                using (var server = new TcpListener())
                {
                    server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000));
                    server.Listen();

                    using (var client = await server.AcceptAsync())
                    {
                        var str = await client.ReadStringAsync();
                        Console.WriteLine("From Client: {0}", str);
                        client.Write(response);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Server Exception:");
                Console.WriteLine(e);
            }
        }
        public static async Task Client()
        {
            try
            {
                using (var client = new Tcp())
                {
                    await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000));
                    client.Write("aha!");
                    var str = await client.ReadStringAsync();
                    Console.WriteLine("From Server: {0}", str);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Client Exception:");
                Console.WriteLine(e);
            }
        }
        public static async Task<string> ReadStringAsync(this IUVStream<ArraySegment<byte>> stream)
        {
            return await ReadStringAsync(stream, Encoding.GetEncoding(0));
        }

        public static async Task<string> ReadStringAsync(this IUVStream<ArraySegment<byte>> stream, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentException("encoding");
            }
            var buffer = await stream.ReadStructAsync();
            if (!buffer.HasValue)
            {
                return null;
            }
            return encoding.GetString(buffer.Value.Array);
        }
    }
}
