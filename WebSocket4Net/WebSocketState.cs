using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    public enum WebSocketState
    {
        None = -1,
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3
    }
}
