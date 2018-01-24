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