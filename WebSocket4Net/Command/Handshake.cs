using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net.Command
{
    public class Handshake : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            if (!session.ProtocolProcessor.VerifyHandshake(commandInfo))
            {
                session.Close();
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
