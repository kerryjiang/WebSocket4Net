using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            WebSocket websocket = new WebSocket("ws://127.0.0.1:2012/websocket", "basic", WebSocketVersion.DraftHybi00);
            websocket.Opened += new EventHandler(websocket_Opened);

            websocket.Open();

            while (true)
            {
                var line = Console.ReadLine();

                if (line.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    websocket.Close();
                    break;
                }
            }
        }

        static void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("handshake succeded");
        }
    }
}
