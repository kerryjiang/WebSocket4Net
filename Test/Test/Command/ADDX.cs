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
    public class ADDX : JsonSubCommand<AddIn>
    {
        protected override void ExecuteJsonCommand(WebSocketSession session, AddIn commandInfo)
        {
            Async.Run((o) => Calculate(o), new
            {
                Session = session,
                Parameter = commandInfo,
                Token = session.CurrentToken
            });
        }

        private void Calculate(dynamic state)
        {
            var session = state.Session as WebSocketSession;
            var parameter = state.Parameter as AddIn;

            var result = new AddOut { Result = parameter.A + parameter.B };

            Thread.Sleep(2000);

            this.SendJsonResponseWithToken(session, state.Token, result);
        }
    }
}
