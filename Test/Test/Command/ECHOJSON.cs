using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket.SubProtocol;
using WebSocket4Net.Test.Json;
using SuperWebSocket;

namespace WebSocket4Net.Test.Command
{
    public class ECHOJSON : JsonSubCommand<AddIn>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, AddIn commandInfo)
        {
            SendJsonResponse(session, commandInfo);
        }
    }
}
