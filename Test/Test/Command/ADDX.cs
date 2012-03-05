using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.Common;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using WebSocket4Net.Test.Json;
using System.Threading;

namespace WebSocket4Net.Test.Command
{
    public class ADDX : AsyncJsonSubCommand<AddIn>
    {
        protected override void ExecuteAsyncJsonCommand(WebSocketSession session, string token, AddIn commandInfo)
        {
            var result = new AddOut { Result = commandInfo.A + commandInfo.B };

            Thread.Sleep(2000);

            this.SendJsonResponse(session, token, result);
        }
    }
}
