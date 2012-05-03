using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSocket4Net
{
    public partial class JsonWebSocket
    {
        /// <summary>
        /// Gets or sets a value indicating whether [allow unstrusted certificate] when connect a secure websocket uri.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [allow unstrusted certificate]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowUnstrustedCertificate
        {
            get { return m_WebSocket.AllowUnstrustedCertificate; }
            set
            {
                m_WebSocket.AllowUnstrustedCertificate = value;
            }
        }
    }
}
