using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Browser;
using SuperSocket.ClientEngine;

namespace WebSocket4Net.JsBridge
{
    [ScriptableType]
    public class WebSocketBridge
    {
        private WebSocket m_WebSocket;

        private AsyncOperation m_AsyncOper;

        private const int m_DefaultAutoSendPingInterval = 60;

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri)
        {
            Open(uri, string.Empty, ClientAccessPolicyProtocol.Http, true, m_DefaultAutoSendPingInterval);
        }

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri, string protocol)
        {
            Open(uri, protocol, ClientAccessPolicyProtocol.Http, true, m_DefaultAutoSendPingInterval);
        }

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri, string protocol, ClientAccessPolicyProtocol policyProtocol)
        {
            Open(uri, protocol, policyProtocol, true, m_DefaultAutoSendPingInterval);
        }

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri, string protocol, ClientAccessPolicyProtocol policyProtocol, bool enableAutoSendPing)
        {
            Open(uri, protocol, policyProtocol, enableAutoSendPing, m_DefaultAutoSendPingInterval);
        }

        [ScriptableMember(ScriptAlias = "open")]
        public void Open(string uri, string protocol, ClientAccessPolicyProtocol policyProtocol, bool enableAutoSendPing, int autoSendPingInterval)
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
            m_WebSocket.Error += new EventHandler<ErrorEventArgs>(m_WebSocket_Error);
            m_WebSocket.ClientAccessPolicyProtocol = (policyProtocol == ClientAccessPolicyProtocol.Http) ? SocketClientAccessPolicyProtocol.Http : SocketClientAccessPolicyProtocol.Tcp;
            m_WebSocket.EnableAutoSendPing = enableAutoSendPing;
            m_WebSocket.AutoSendPingInterval = autoSendPingInterval;
            m_WebSocket.Open();
        }

        void m_WebSocket_Error(object sender, ErrorEventArgs e)
        {
            m_AsyncOper.Post((s) => FireError((string)s), e.Exception.Message);
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

        [ScriptableMember(ScriptAlias = "onerror")]
        public event EventHandler<MessageEventArgs> Error;

        private void FireError(string error)
        {
            var handler = Error;

            if (handler == null)
                return;

            handler(null, new MessageEventArgs { Data = error });
        }
    }
}
