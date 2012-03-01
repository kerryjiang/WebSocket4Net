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

        public JsonWebSocket(string uri)
            : this(uri, string.Empty)
        {

        }

        public JsonWebSocket(string uri, WebSocketVersion version)
            : this(uri, string.Empty, null, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, null, WebSocketVersion.None)
        {

        }

        public JsonWebSocket(string uri, List<KeyValuePair<string, string>> cookies)
            : this(uri, string.Empty, cookies, WebSocketVersion.None)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies)
            : this(uri, subProtocol, cookies, WebSocketVersion.None)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, WebSocketVersion version)
            : this(uri, subProtocol, null, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, WebSocketVersion version)
            : this(uri, subProtocol, cookies, null, string.Empty, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, WebSocketVersion version)
        {
            m_WebSocket = new WebSocket(uri, subProtocol, cookies, customHeaderItems, userAgent, version);
            m_WebSocket.Closed += new EventHandler(m_WebSocket_Closed);
            m_WebSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(m_WebSocket_MessageReceived);
            m_WebSocket.Opened += new EventHandler(m_WebSocket_Opened);
            m_WebSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
        }

        public int ReceiveBufferSize
        {
            get { return m_WebSocket.ReceiveBufferSize; }
            set { m_WebSocket.ReceiveBufferSize = value; }
        }

        public void Open()
        {
            if (m_WebSocket.State == WebSocketState.None)
                m_WebSocket.Open();
        }

        public void Close()
        {
            if (m_WebSocket == null)
                return;

            if (m_WebSocket.State == WebSocketState.Open || m_WebSocket.State == WebSocketState.Connecting)
                m_WebSocket.Close();
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
            string token = string.Empty;

            if (spacePos > 0)
            {
                name = e.Message.Substring(0, spacePos);
                parameter = e.Message.Substring(spacePos + 1);

                if (parameter[0] != '{')
                {
                    spacePos = parameter.IndexOf(' ');

                    if (spacePos > 0)
                    {
                        token = parameter.Substring(0, spacePos);
                        parameter = parameter.Substring(spacePos + 1);
                    }
                }
            }
            else
            {
                name = e.Message;
                parameter = string.Empty;
            }

            IJsonExecutor executor = GetExecutor(name, token);

            if (executor != null)
            {
                if(!executor.Type.IsPrimitive)
                    executor.Execute(JsonConvert.DeserializeObject(parameter, executor.Type));
                else
                    executor.Execute(Convert.ChangeType(parameter, executor.Type, null));
            }
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

        public void On<T>(string name, Action<T> executor)
        {
            RegisterExecutor<T>(name, string.Empty, executor);
        }

        public void Send(string name, object content)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if(content != null)
                m_WebSocket.Send(string.Format(m_QueryTemplateB, name, JsonConvert.SerializeObject(content)));
            else
                m_WebSocket.Send(name);
        }

        private static Random m_Random = new Random();

        private const string m_QueryTemplateA = "{0} {1} {2}";
        private const string m_QueryTemplateB = "{0} {1}";
        private const string m_QueryTokenTemplate = "{0}-{1}";

        public void Query<T>(string name, object content, Action<T> executor)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            int token = m_Random.Next(1000, 9999);

            RegisterExecutor<T>(name, token.ToString(), executor);

            if (content != null)
                m_WebSocket.Send(string.Format(m_QueryTemplateA, name, token, JsonConvert.SerializeObject(content)));
            else
                m_WebSocket.Send(string.Format(m_QueryTemplateB, name, token));
        }

        private Dictionary<string, IJsonExecutor> m_ExecutorDict = new Dictionary<string, IJsonExecutor>(StringComparer.OrdinalIgnoreCase);

        void RegisterExecutor<T>(string name, string token, Action<T> executor)
        {
            lock (m_ExecutorDict)
            {
                if (string.IsNullOrEmpty(token))
                    m_ExecutorDict.Add(name, new JsonExecutor<T>(executor));
                else
                    m_ExecutorDict.Add(string.Format(m_QueryTokenTemplate, name, token), new JsonExecutor<T>(executor));
            }
        }

        IJsonExecutor GetExecutor(string name, string token)
        {
            string key = name;
            bool removeExecutor = false;

            if (!string.IsNullOrEmpty(token))
            {
                key = string.Format(m_QueryTokenTemplate, name, token);
                removeExecutor = true;
            }

            lock (m_ExecutorDict)
            {
                IJsonExecutor executor;

                if (!m_ExecutorDict.TryGetValue(key, out executor))
                    return null;

                if (removeExecutor)
                {
                    m_ExecutorDict.Remove(key);
                }

                return executor;
            }
        }
    }
}
