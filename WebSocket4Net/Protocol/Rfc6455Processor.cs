using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455
    /// </summary>
    class Rfc6455Processor : DraftHybi10Processor
    {
        public Rfc6455Processor()
            : base(WebSocketVersion.Rfc6455, new CloseStatusCodeRfc6455())
        {

        }
    }
}
