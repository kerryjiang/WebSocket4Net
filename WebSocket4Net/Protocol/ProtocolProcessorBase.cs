using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net.Protocol
{
    abstract class ProtocolProcessorBase : IProtocolProcessor
    {
        protected WebSocket WebSocket { get; private set; }

        public void Initialize(WebSocket websocket)
        {
            WebSocket = websocket;
        }

        public abstract void SendHandshake();

        public abstract ReaderBase CreateHandshakeReader();

        public abstract bool VerifyHandshake(WebSocketCommandInfo handshakeInfo);

        public abstract void SendMessage(string message);

        public abstract void SendCloseHandshake(string closeReason);

        public abstract void SendPing(string ping);
    }
}
