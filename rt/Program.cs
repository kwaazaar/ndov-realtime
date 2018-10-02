using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace rt
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Arguments: <nodeAddr> <subscr...>");
                Console.WriteLine("Eg: tcp://node03.kv7.openov.nl:7817 /GOVI/KV8");
                return -1;
            }

            var nodeAddr = args[0];
            var subscriptions = args.Skip(1).ToArray();

            using (var socket = new SubscriberSocket())
            {
                int msgCount = 0;
                socket.Connect(nodeAddr);
                try
                {
                    subscriptions.All((s) =>
                    {
                        socket.Subscribe(s);
                        return true;
                    });

                    // Download just 20 messages, it's just a test tool...
                    while (msgCount < 20)
                    {
                        var messageList = socket.ReceiveMultipartBytes(2);
                        msgCount++;
                        var msg1 = Encoding.UTF8.GetString(messageList[0]);
                        using (GZipStream stream = new GZipStream(new MemoryStream(messageList[1]), CompressionMode.Decompress))
                        using (var sr = new StreamReader(stream))
                        {
                            var msg2 = sr.ReadToEnd();
                            Console.Write($"{msg1} - {msg2.Length} chars...");
                            var filename = msg1.Substring(1).Replace('/', '-') + $"{msgCount}.xml";
                            File.WriteAllText(filename, msg2, Encoding.UTF8);
                            Console.WriteLine();
                        }
                    }
                }
                finally
                {
                    socket.Disconnect(nodeAddr);
                    socket.Close();
                }
            }

            return 0;
        }
    }
}
