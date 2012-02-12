using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        public WebSocket(string uri, string subProtocol, WebSocketVersion version)
            : this(uri, subProtocol, null, null, string.Empty, version)
        {

        }

        public WebSocket(string uri, string subProtocol = "", List<KeyValuePair<string, string>> cookies = null, List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", WebSocketVersion version = WebSocketVersion.None)
        {
            Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, version);
        }
    }
}
