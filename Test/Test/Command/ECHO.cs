using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;

namespace WebSocket4Net.Test
{
    public class ECHO : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            foreach (var p in requestInfo.Data.Split(' '))
            {
                session.SendResponse(p);
            }
        }
    }
}
