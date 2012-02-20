using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using WebSocket4Net.Test.Json;

namespace WebSocket4Net.Test.Command
{
    public class ADD : JsonSubCommand<AddIn>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, AddIn commandInfo)
        {
            SendJsonResponse(session, new AddOut { Result = commandInfo.A + commandInfo.B });
        }
    }
}
