using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol
{
    class DraftHybi10HandshakeReader : HandshakeReader
    {
        public DraftHybi10HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            var cmdInfo = base.GetCommandInfo(readBuffer, offset, length, out left);

            if (cmdInfo == null)
                return null;

            //If bad request, NextCommandReader will still be this HandshakeReader
            if (!BadRequestCode.Equals(cmdInfo.Key))
                NextCommandReader = new DraftHybi10DataReader();
            
            return cmdInfo;
        }
    }
}
