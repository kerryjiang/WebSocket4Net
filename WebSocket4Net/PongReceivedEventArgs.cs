using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    public class PongReceivedEventArgs : EventArgs
    {
        public PongReceivedEventArgs(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; private set; }
    }
}
