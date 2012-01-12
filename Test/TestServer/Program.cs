using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using System.Reflection;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            LogUtil.Setup(new ConsoleLogger());

            var websocketServer = new WebSocketServer(new BasicSubProtocol("Basic", new List<Assembly> { typeof(Program).Assembly }));
            websocketServer.NewDataReceived += new SessionEventHandler<WebSocketSession, byte[]>(websocketServer_NewDataReceived);
            websocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 2012,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Async,
                Name = "SuperWebSocket Server"
            }, SocketServerFactory.Instance);

            websocketServer.Start();

            Console.WriteLine("The server is started, press 'Q' to quit the server!");

            while (true)
            {
                var line = Console.ReadLine();

                if (line.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    websocketServer.Stop();
                    break;
                }
            }
        }

        static void websocketServer_NewDataReceived(WebSocketSession session, byte[] e)
        {
            session.SendResponse(e);
        }
    }
}
