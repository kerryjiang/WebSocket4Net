using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;

namespace TestServer.Test
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, StringCommandInfo commandInfo)
        {
            foreach(var p in commandInfo.Parameters)
            {
                session.SendResponse(p);
            }
        }
    }
}
