using System;
using System.Collections.Generic;


namespace WebSocket4Net.Protocol
{
    public interface IProtocolProcessor
    {
        void SendHandshake(WebSocket websocket);

        bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description);

        ReaderBase CreateHandshakeReader(WebSocket websocket);

        void SendMessage(WebSocket websocket, string message);

        void SendData(WebSocket websocket, byte[] data, int offset, int length);

        void SendData(WebSocket websocket, IList<ArraySegment<byte>> segments);

        void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason);

        void SendPing(WebSocket websocket, string ping);

        void SendPong(WebSocket websocket, string pong);

        bool SupportBinary { get; }

        bool SupportPingPong { get; }

        ICloseStatusCode CloseStatusCode { get; }

        WebSocketVersion Version { get; }
    }
}
