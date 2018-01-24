namespace WebSocket4Net
{
    public enum WebSocketState : int
    {
        None = WebSocketStateConst.None,
        Connecting = WebSocketStateConst.Connecting,
        Open = WebSocketStateConst.Open,
        Closing = WebSocketStateConst.Closing,
        Closed = WebSocketStateConst.Closed
    }

    internal static class WebSocketStateConst
    {
        public const int None = -1;

        public const int Connecting = 0;

        public const int Open = 1;

        public const int Closing = 2;

        public const int Closed = 3;
    }
}