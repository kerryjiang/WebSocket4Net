namespace WebSocket4Net.Protocol.FramePartReader
{
    internal interface IDataFramePartReader
    {
        int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);
    }
}