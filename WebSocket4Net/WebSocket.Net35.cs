using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        private bool m_AllowUnstrustedCertificate;

        /// <summary>
        /// Gets or sets a value indicating whether [allow unstrusted certificate] when connect a secure websocket uri.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow unstrusted certificate]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowUnstrustedCertificate
        {
            get { return m_AllowUnstrustedCertificate; }
            set
            {
                m_AllowUnstrustedCertificate = value;

                var client = Client as SslStreamTcpSession;

                if(client != null)
                {
                    client.AllowUnstrustedCertificate = m_AllowUnstrustedCertificate;
                }
            }
        }

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
            : this(uri, subProtocol, cookies, customHeaderItems, userAgent, string.Empty, version)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, string origin, WebSocketVersion version)
        {
            Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version);
        }
    }
}