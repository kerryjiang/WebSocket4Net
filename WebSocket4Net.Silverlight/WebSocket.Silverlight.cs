using System;
using System.Net;
using System.Collections.Generic;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
        public WebSocket(string uri, string subProtocol = "", string cookies = "", List<KeyValuePair<string, string>> customHeaderItems = null, string userAgent = "", WebSocketVersion version = WebSocketVersion.None)
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

            Initialize(uri, subProtocol, cookieList, customHeaderItems, userAgent, version);
        }
    }
}
