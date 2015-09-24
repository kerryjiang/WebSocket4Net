﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        private static List<KeyValuePair<string, string>> EmptyCookies = null;

#if !NETFX_CORE

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

                if (client != null)
                {
                    client.AllowUnstrustedCertificate = m_AllowUnstrustedCertificate;
                }
            }
        }

#endif

        public WebSocket(string uri, string subProtocol, WebSocketVersion version, SslProtocols sslProtocols = SslProtocols.Default)
            : this(uri, subProtocol, EmptyCookies, null, string.Empty, string.Empty, version, sslProtocols)
        {

        }

        public WebSocket(string uri, string subProtocol = "", List<KeyValuePair<string, string>> cookies = null, List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", string origin = "", WebSocketVersion version = WebSocketVersion.None, SslProtocols sslProtocols = SslProtocols.Default)
            : this(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version, null, sslProtocols)
        {

        }

        public WebSocket(string uri, string subProtocol = "", List<KeyValuePair<string, string>> cookies = null, List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", string origin = "", WebSocketVersion version = WebSocketVersion.None, EndPoint httpConnectProxy = null, SslProtocols sslProtocols = SslProtocols.Default)
        {
            Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version, httpConnectProxy, sslProtocols);
        }
    }
}
