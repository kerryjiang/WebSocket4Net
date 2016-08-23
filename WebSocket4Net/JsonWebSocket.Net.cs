using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    public partial class JsonWebSocket
    {
        /// <summary>
        /// Get websocket connection security options
        /// </summary>
        public SecurityOption Security
        {
            get { return m_WebSocket.Security; }
        }
    }
}
