
namespace WebSocket4Net.Protocol
{
    public interface IProtocolProcessor
    {
        void SendHandshake(WebSocket websocket);

        bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description);

        ReaderBase CreateHandshakeReader(WebSocket websocket);

        void SendMessage(WebSocket websocket, string message);

        void SendData(WebSocket websocket, byte[] data, int offset, int length);

        void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason);

        void SendPing(WebSocket websocket, string ping);

        bool SupportBinary { get; }

        ICloseStatusCode CloseStatusCode { get; }

        WebSocketVersion Version { get; }
    }
}
