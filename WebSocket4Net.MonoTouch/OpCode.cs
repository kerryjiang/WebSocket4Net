using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net
{
    public class OpCode
    {
        public const int Handshake = -1; // defined by myself
        public const int Text = 1;
        public const int Binary = 2;
        public const int Close = 8;
        public const int Ping = 9;
        public const int Pong = 10;
        public const int BadRequest = 400;
    }
}
