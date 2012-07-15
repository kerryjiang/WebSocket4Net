using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Command
{
    public class Close : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            //Close handshake was sent from client side, now got a handshake response
            if (session.State == WebSocketState.Closing)
            {
                //Not NormalClosure
                if (commandInfo.CloseStatusCode != session.ProtocolProcessor.CloseStatusCode.NormalClosure &&
                    (commandInfo.CloseStatusCode > 0 || !string.IsNullOrEmpty(commandInfo.Text)))
                {
                    session.FireError(new Exception(string.Format("{0}: {1}", commandInfo.CloseStatusCode, commandInfo.Text)));
                }
                session.CloseWithoutHandshake();
                return;
            }

            //Got server side closing handshake request, send response now
            var statusCode = commandInfo.CloseStatusCode;

            if (statusCode <= 0)
                statusCode = session.ProtocolProcessor.CloseStatusCode.NoStatusCode;

            session.Close(statusCode, commandInfo.Text);
        }

        public override string Name
        {
            get { return OpCode.Close.ToString(); }
        }
    }
}
