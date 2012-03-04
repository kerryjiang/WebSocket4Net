using System;
using System.Net;
using System.Windows.Browser;
using System.ComponentModel;

namespace WebSocket4Net.JsBridge
{
    [ScriptableType]
    public class WebSocketBridge
    {
        private WebSocket m_WebSocket;

        private AsyncOperation m_AsyncOper;

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri)
        {
            Open(uri, string.Empty);
        }

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri, string protocol)
        {
            m_AsyncOper = AsyncOperationManager.CreateOperation(null);

            //pass in Origin
            var hostName = HtmlPage.Document.DocumentUri.Host;
            var port = HtmlPage.Document.DocumentUri.Port;

            string origin = hostName;

            if (port != 80)
                origin += ":" + port;

            m_WebSocket = new WebSocket(uri, protocol, cookies: HtmlPage.Document.Cookies, origin: origin);
            m_WebSocket.Opened += new EventHandler(m_WebSocket_Opened);
            m_WebSocket.Closed += new EventHandler(m_WebSocket_Closed);
            m_WebSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(m_WebSocket_MessageReceived);
            m_WebSocket.Open();
        }

        void m_WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_AsyncOper.Post((s) => FireMessageReceived((string)s), e.Message);
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            m_AsyncOper.Post((s) => FireClosed(), null);
        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            m_AsyncOper.Post((s) => FireOpened(), null);
        }

        [ScriptableMember(ScriptAlias = "send")]
        public void Send(string message)
        {
            if (m_WebSocket != null && m_WebSocket.State == WebSocketState.Open)
            {
                m_WebSocket.Send(message);
            }
        }

        [ScriptableMember(ScriptAlias = "close")]
        public void Close()
        {
            if (m_WebSocket != null)
            {
                if (m_WebSocket.State != WebSocketState.Closing && m_WebSocket.State != WebSocketState.Closed)
                    m_WebSocket.Close();
            }
        }

        [ScriptableMember(ScriptAlias = "onopen")]
        public event EventHandler Opened;

        private void FireOpened()
        {
            var handler = Opened;

            if (handler == null)
                return;

            handler(null, null);
        }

        [ScriptableMember(ScriptAlias = "onclose")]
        public event EventHandler Closed;

        private void FireClosed()
        {
            var handler = Closed;

            if (handler == null)
                return;

            handler(null, null);
        }

        [ScriptableMember(ScriptAlias = "onmessage")]
        public event EventHandler<MessageEventArgs> MessageReceived;

        private void FireMessageReceived(string message)
        {
            var handler = MessageReceived;

            if (handler == null)
                return;

            handler(null, new MessageEventArgs { Data = message });
        }
    }
}
