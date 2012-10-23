using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    abstract class ProtocolProcessorBase : IProtocolProcessor
    {
        protected const string HeaderItemFormat = "{0}: {1}";

        public ProtocolProcessorBase(WebSocketVersion version, ICloseStatusCode closeStatusCode)
        {
            CloseStatusCode = closeStatusCode;
            Version = version;
            VersionTag = ((int)version).ToString();
        }

        public abstract void SendHandshake(WebSocket websocket);

        public abstract ReaderBase CreateHandshakeReader(WebSocket websocket);

        public abstract bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description);

        public abstract void SendMessage(WebSocket websocket, string message);

        public abstract void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason);

        public abstract void SendPing(WebSocket websocket, string ping);

        public abstract void SendPong(WebSocket websocket, string pong);

        public abstract void SendData(WebSocket websocket, byte[] data, int offset, int length);

        public abstract void SendData(WebSocket websocket, IList<ArraySegment<byte>> segments);

        public abstract bool SupportBinary { get; }

        public abstract bool SupportPingPong { get; }

        public ICloseStatusCode CloseStatusCode { get; private set; }

        public WebSocketVersion Version { get; private set; }

        protected string VersionTag { get; private set; }
    }
}
