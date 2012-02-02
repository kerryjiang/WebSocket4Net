using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.Protocol
{
    public interface IProtocolProcessor
    {
        void Initialize(WebSocket websocket);

        void SendHandshake();

        bool VerifyHandshake(WebSocketCommandInfo handshakeInfo);

        ReaderBase CreateHandshakeReader();

        void SendMessage(string message);

        void SendData(byte[] data, int offset, int length);

        void SendCloseHandshake(int statusCode, string closeReason);

        void SendPing(string ping);

        bool SupportBinary { get; }

        ICloseStatusCode CloseStatusCode { get; }
    }
}
