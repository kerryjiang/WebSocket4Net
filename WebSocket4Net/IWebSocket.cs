using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Net;

namespace WebSocket4Net
{
    public interface IWebSocket
    {
        int ReceiveBufferSize { get; set; }

        SecurityOption Security { get; }

        EndPoint LocalEndPoint { get; set; }

        bool NoDelay { get; set; }

        IProxyConnector Proxy { get; set; }

        bool Handshaked { get; }

        WebSocketState State { get; }

        bool SupportBinary { get; }

        int AutoSendPingInterval { get; set; }

        bool EnableAutoSendPing { get; set; }

        DateTime LastActiveTime { get; }

        WebSocketVersion Version { get; }

        void Close(int statusCode, string reason);

        void Close(string reason);

        void Close();

        void Dispose();

        void Open();

        void Send(byte[] data, int offset, int length);

        void Send(string message);

        void Send(IList<ArraySegment<byte>> segments);

        event EventHandler<ErrorEventArgs> Error;
        event EventHandler<DataReceivedEventArgs> DataReceived;
        event EventHandler<MessageReceivedEventArgs> MessageReceived;
        event EventHandler Closed;
        event EventHandler Opened;
    }
}
