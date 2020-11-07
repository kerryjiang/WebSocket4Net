namespace WebSocket4Net
{
    public enum WebSocketState
    {
        None,
        Connecting,
        Open,
        CloseSent,
        CloseReceived,
        Closed
    }
}