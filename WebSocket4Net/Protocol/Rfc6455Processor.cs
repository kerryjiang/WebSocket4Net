using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455
    /// </summary>
    class Rfc6455Processor : DraftHybi10Processor
    {
        public Rfc6455Processor()
            : base(13, new CloseStatusCodeRfc6455())
        {

        }
    }
}
