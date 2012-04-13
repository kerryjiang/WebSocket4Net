using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    interface IJsonExecutor
    {
        Type Type { get; }

        void Execute(JsonWebSocket websocket, string token, object param);
    }
}
