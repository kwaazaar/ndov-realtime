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
        static void Main(string[] args)
        {
            using (var sub = new SubscriberSocket())
            {
                List<string[]> messages = new List<string[]>();

                const string nodeAddr = "tcp://node03.kv7.openov.nl:7817";
                sub.Connect(nodeAddr);
                try
                {
                    sub.Subscribe("/GOVI/KV8");
                    sub.Subscribe("/GOVI/KV7calendar");
                    sub.Subscribe("/GOVI/KV7planning");

                    while (messages.Count < 20)
                    {
                        var messageList = sub.ReceiveMultipartBytes(2);
                        var msg1 = Encoding.UTF8.GetString(messageList[0]);
                        using (GZipStream stream = new GZipStream(new MemoryStream(messageList[1]), CompressionMode.Decompress))
                        using (var sr = new StreamReader(stream))
                        {
                            var msg2 = sr.ReadToEnd();
                            messages.Add(new string[] { msg1, msg2 });

                            if (msg1.StartsWith("/GOVI/KV8generalmessages"))
                            {
                                msg2.Split('\n').Skip(1).ToList().ForEach(s => Console.WriteLine(s));
                            }
                        }
                    }
                }
                finally
                {
                    //The biggest difference between the two libraries. You can disconnect, close and terminate manually.
                    //So make sure not to forget this.
                    sub.Disconnect(nodeAddr);
                    sub.Close();
                }
            }




        }
    }
}
