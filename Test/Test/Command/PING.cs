using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using SuperSocket.SocketBase.Command;

namespace WebSocket4Net.Test.Command
{
    public class PING : SubCommandBase
    {
        public override void ExecuteCommand(WebSocketSession session, SubRequestInfo requestInfo)
        {
            session.ProtocolProcessor.SendPing(session, new byte[0]);
        }
    }
}
