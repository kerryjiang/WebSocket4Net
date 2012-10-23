using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Command
{
    public class Handshake : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            string description;

            if (!session.ProtocolProcessor.VerifyHandshake(session, commandInfo, out description))
            {
                session.Close(session.ProtocolProcessor.CloseStatusCode.ProtocolError, description);
                return;
            }

            session.OnHandshaked();
        }

        public override string Name
        {
            get { return OpCode.Handshake.ToString(); }
        }
    }
}
