﻿using System;

namespace WebSocket4Net.Command
{
    public class Ping : WebSocketCommandBase
    {
        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.LastActiveTime = DateTime.Now;
            session.ProtocolProcessor.SendPong(session, commandInfo.Text);
        }

        public override string Name
        {
            get { return OpCode.Ping.ToString(); }
        }
    }
}