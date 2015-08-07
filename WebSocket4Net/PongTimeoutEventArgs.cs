using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    public class PongTimeoutEventArgs : EventArgs
    {
        public PongTimeoutEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }
}
