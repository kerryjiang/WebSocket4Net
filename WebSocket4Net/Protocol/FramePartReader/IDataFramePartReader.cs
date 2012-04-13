using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol.FramePartReader
{
    interface IDataFramePartReader
    {
        int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);
    }
}
