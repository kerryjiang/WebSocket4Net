using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        public WebSocket(string uri)
            : this(uri, string.Empty)
        {
        }

        public WebSocket(string uri, WebSocketVersion version)
            : this(uri, string.Empty, null, version)
        {

        }

        public WebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, null, WebSocketVersion.None)
        {
        }

        public WebSocket(string uri, List<KeyValuePair<string, string>> cookies)
            : this(uri, string.Empty, cookies, WebSocketVersion.None)
        {
        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies)
            : this(uri, subProtocol, cookies, WebSocketVersion.None)
        {

        }

        public WebSocket(string uri, string subProtocol, WebSocketVersion version)
            : this(uri, subProtocol, null, version)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, WebSocketVersion version)
            : this(uri, subProtocol, cookies, new List<KeyValuePair<string, string>>(), null, version)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, string userAgent, WebSocketVersion version)
            : this(uri, subProtocol, cookies, null, userAgent, version)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, WebSocketVersion version)
        {
            Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, version);
        }
    }
}