using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketEngine;
using System.Diagnostics;
using SuperWebSocket;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var firstArg = string.Empty;

            if (args != null && args.Length > 0)
            {
                firstArg = args[0];
            }

            if ("stop".Equals(firstArg, StringComparison.OrdinalIgnoreCase))
            {
                StopServer();
                return;
            }

            var bootstrap = BootstrapFactory.CreateBootstrap();

            if (!bootstrap.Initialize())
                return;

            foreach (var server in bootstrap.AppServers.OfType<WebSocketServer>())
            {
                server.NewDataReceived += new SuperSocket.SocketBase.SessionHandler<WebSocketSession, byte[]>(server_NewDataReceived);
            }

            var result = bootstrap.Start();

            Console.WriteLine("Start result: {0}", result);

            while (Console.ReadKey().KeyChar != 'q')
                continue;

            bootstrap.Stop();
        }

        static void server_NewDataReceived(WebSocketSession session, byte[] value)
        {
            session.Send(value, 0, value.Length);
            session.Logger.Info(value.Length);
        }

        static void StopServer()
        {
            var currentProcess = Process.GetCurrentProcess();
            var targetProcess = Process.GetProcessesByName(currentProcess.ProcessName).FirstOrDefault(p => p.Id != currentProcess.Id);

            if (targetProcess != null)
            {
                targetProcess.StandardInput.Write('q');
                targetProcess.StandardInput.Flush();
            }
        }
    }
}
