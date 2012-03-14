using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies = null, List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", string origin = "", WebSocketVersion version = WebSocketVersion.None)
        {
            Initialize(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version);
        }

        public WebSocket(string uri, string subProtocol = "", string cookies = "", List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", string origin = "", WebSocketVersion version = WebSocketVersion.None)
        {
            List<KeyValuePair<string, string>> cookieList = null;

            if (!string.IsNullOrEmpty(cookies))
            {
                cookieList = new List<KeyValuePair<string, string>>();

                string[] pairs = cookies.Split(';');

                int pos;
                string key, value;

                foreach (var p in pairs)
                {
                    pos = p.IndexOf('=');
                    if (pos > 0)
                    {
                        key = p.Substring(0, pos).Trim();
                        pos += 1;
                        if (pos < p.Length)
                            value = p.Substring(pos).Trim();
                        else
                            value = string.Empty;

                        cookieList.Add(new KeyValuePair<string, string>(key, Uri.UnescapeDataString(value)));
                    }
                }
            }

            Initialize(uri, subProtocol, cookieList, customHeaderItems, userAgent, origin, version);
        }

        /// <summary>
        /// Windows Phone doesn't have this feature
        /// </summary>
#if !WINDOWS_PHONE
        private SocketClientAccessPolicyProtocol m_ClientAccessPolicyProtocol = SocketClientAccessPolicyProtocol.Http;

        public SocketClientAccessPolicyProtocol ClientAccessPolicyProtocol
        {
            get { return m_ClientAccessPolicyProtocol; }
            set
            {
                if (this.State != WebSocketState.None)
                    throw new Exception("You cannot set ClientAccessPolicyProtocol after the connection was established!");

                m_ClientAccessPolicyProtocol = value;
            }
        }
#endif
    }
}
