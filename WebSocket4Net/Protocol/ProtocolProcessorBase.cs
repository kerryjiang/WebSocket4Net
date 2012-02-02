using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    abstract class ProtocolProcessorBase : IProtocolProcessor
    {
        public ProtocolProcessorBase(ICloseStatusCode closeStatusCode)
        {
            CloseStatusCode = closeStatusCode;
        }

        protected WebSocket WebSocket { get; private set; }

        protected TcpClientSession Client { get; private set; }

        public void Initialize(WebSocket websocket)
        {
            WebSocket = websocket;
            Client = websocket;
        }

        public abstract void SendHandshake();

        public abstract ReaderBase CreateHandshakeReader();

        public abstract bool VerifyHandshake(WebSocketCommandInfo handshakeInfo);

        public abstract void SendMessage(string message);

        public abstract void SendCloseHandshake(int statusCode, string closeReason);

        public abstract void SendPing(string ping);

        public abstract void SendData(byte[] data, int offset, int length);

        public abstract bool SupportBinary { get; }

        public ICloseStatusCode CloseStatusCode { get; private set; }
    }
}
