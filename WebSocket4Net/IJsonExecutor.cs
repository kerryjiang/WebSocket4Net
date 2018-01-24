using System;

namespace WebSocket4Net
{
    internal interface IJsonExecutor
    {
        Type Type { get; }

        void Execute(JsonWebSocket websocket, string token, object param);
    }
}