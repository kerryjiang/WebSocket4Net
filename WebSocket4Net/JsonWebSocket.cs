using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WebSocket4Net
{
    public class JsonWebSocket
    {
        private WebSocket m_WebSocket;

        public WebSocketState State
        {
            get { return m_WebSocket.State; }
        }

        private Dictionary<string, IJsonExecutor> m_ExecutorDict = new Dictionary<string, IJsonExecutor>(StringComparer.OrdinalIgnoreCase);

        public JsonWebSocket(string uri)
            : this(uri, string.Empty)
        {

        }

        public JsonWebSocket(string uri, WebSocketVersion version)
            : this(uri, string.Empty, null, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, null, WebSocketVersion.DraftHybi10)
        {

        }

        public JsonWebSocket(string uri, List<KeyValuePair<string, string>> cookies)
            : this(uri, string.Empty, cookies, WebSocketVersion.DraftHybi00)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies)
            : this(uri, subProtocol, cookies, WebSocketVersion.DraftHybi00)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, WebSocketVersion version)
            : this(uri, subProtocol, null, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, WebSocketVersion version)
        {
            m_WebSocket = new WebSocket(uri, subProtocol, cookies, version);
            m_WebSocket.Closed += new EventHandler(m_WebSocket_Closed);
            m_WebSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(m_WebSocket_MessageReceived);
            m_WebSocket.Opened += new EventHandler(m_WebSocket_Opened);
            m_WebSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
        }

        private EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> m_Error;

        public event EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> Error
        {
            add { m_Error += value; }
            remove { m_Error -= value; }
        }

        void m_WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (m_Error == null)
                return;

            m_Error(this, e);
        }

        private EventHandler m_Opened;

        public event EventHandler Opened
        {
            add { m_Opened += value; }
            remove { m_Opened -= value; }
        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            if (m_Opened == null)
                return;

            m_Opened(this, e);
        }

        void m_WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Message))
                return;

            int spacePos = e.Message.IndexOf(' ');

            string name;
            string parameter;

            if (spacePos > 0)
            {
                name = e.Message.Substring(0, spacePos);
                parameter = e.Message.Substring(spacePos + 1);
            }
            else
            {
                name = e.Message;
                parameter = string.Empty;
            }

            IJsonExecutor executor;
            if (!m_ExecutorDict.TryGetValue(name, out executor))
                return;

            executor.Execute(JsonConvert.DeserializeObject(parameter, executor.Type));
        }

        private EventHandler m_Closed;

        public event EventHandler Closed
        {
            add { m_Closed += value; }
            remove { m_Closed -= value; }
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            if (m_Closed == null)
                return;

            m_Closed(this, e);
        }

        public void RegisterHandler<T>(string name, Action<T> executor)
        {
            m_ExecutorDict[name] = new JsonExecutor<T>(executor);
        }

        public void Send(string name, object content)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            m_WebSocket.Send(name + " " + JsonConvert.SerializeObject(content));
        }
    }
}
