using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    /// <summary>
    /// WebSocket client wrapping which serializes/deserializes objects by JSON
    /// </summary>
    public partial class JsonWebSocket
    {
        private WebSocket m_WebSocket;

        /// <summary>
        /// Gets or sets a value indicating whether [enable auto send ping].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable auto send ping]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAutoSendPing
        {
            get { return m_WebSocket.EnableAutoSendPing; }
            set { m_WebSocket.EnableAutoSendPing = value; }
        }

        /// <summary>
        /// Gets or sets the interval of ping auto sending, in seconds.
        /// </summary>
        /// <value>
        /// The auto send ping internal.
        /// </value>
        public int AutoSendPingInterval
        {
            get { return m_WebSocket.AutoSendPingInterval; }
            set { m_WebSocket.AutoSendPingInterval = value; }
        }

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
            : this(uri, subProtocol, cookies, null, string.Empty, string.Empty, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, WebSocketVersion version)
            : this(uri, subProtocol, cookies, customHeaderItems, userAgent, string.Empty, version)
        {

        }

        public JsonWebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> customHeaderItems, string userAgent, string origin, WebSocketVersion version)
        {
            m_WebSocket = new WebSocket(uri, subProtocol, cookies, customHeaderItems, userAgent, origin, version);
            m_WebSocket.Closed += new EventHandler(m_WebSocket_Closed);
            m_WebSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(m_WebSocket_MessageReceived);
            m_WebSocket.Opened += new EventHandler(m_WebSocket_Opened);
            m_WebSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
            m_WebSocket.EnableAutoSendPing = true;
        }

#if SILVERLIGHT && !WINDOWS_PHONE
        public SocketClientAccessPolicyProtocol ClientAccessPolicyProtocol
        {
            get { return m_WebSocket.ClientAccessPolicyProtocol; }
            set { m_WebSocket.ClientAccessPolicyProtocol = value; }
        }
#endif

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

            int pos = e.Message.IndexOf(' ');

            string name;
            string parameter;
            string token = string.Empty;

            if (pos > 0)
            {
                name = e.Message.Substring(0, pos);
                parameter = e.Message.Substring(pos + 1);

                pos = name.IndexOf('-');

                if (pos > 0)
                {
                    token = name.Substring(pos + 1);
                    name = name.Substring(0, pos);
                }
            }
            else
            {
                name = e.Message;
                parameter = string.Empty;
            }

            IJsonExecutor executor = GetExecutor(name, token);

            if (executor == null)
                return;

            object value;

            try
            {
                if (!executor.Type.IsPrimitive)
                    value = DeserializeObject(parameter, executor.Type);
                else
                    value = Convert.ChangeType(parameter, executor.Type, null);
            }
            catch (Exception exc)
            {
                m_WebSocket_Error(this, new ErrorEventArgs(new Exception("DeserializeObject exception", exc)));
                return;
            }

            try
            {
                executor.Execute(this, token, value);
            }
            catch (Exception exce)
            {
                m_WebSocket_Error(this, new ErrorEventArgs(new Exception("Message handling exception", exce)));
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

        /// <summary>
        /// Registers the message handler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The message's name.</param>
        /// <param name="executor">The message handler.</param>
        public void On<T>(string name, Action<T> executor)
        {
            RegisterExecutor<T>(name, string.Empty, new JsonExecutor<T>(executor));
        }


        /// <summary>
        /// Registers the message handler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="executor">The executor.</param>
        public void On<T>(string name, Action<JsonWebSocket, T> executor)
        {
            RegisterExecutor<T>(name, string.Empty, new JsonExecutorWithSender<T>(executor));
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="target">The target object is being serialized.</param>
        /// <returns></returns>
        protected virtual string SerializeObject(object target)
        {
            return SimpleJson.SimpleJson.SerializeObject(target);
        }

        /// <summary>
        /// Deserializes the json string to obeject.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="type">The type of the target object.</param>
        /// <returns></returns>
        protected virtual object DeserializeObject(string json, Type type)
        {
            return SimpleJson.SimpleJson.DeserializeObject(json, type);
        }

        /// <summary>
        /// Sends object with specific name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="content">The object you want to send.</param>
        public void Send(string name, object content)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (content != null)
            {
                if (!content.GetType().IsPrimitive)
                    m_WebSocket.Send(string.Format(m_QueryTemplateC, name, SerializeObject(content)));
                else
                    m_WebSocket.Send(string.Format(m_QueryTemplateC, name, content));
            }
            else
                m_WebSocket.Send(name);
        }

        private static Random m_Random = new Random();

        private const string m_QueryTemplateA = "{0}-{1} {2}"; //With token and content
        private const string m_QueryTemplateB = "{0}-{1}"; //With token
        private const string m_QueryTemplateC = "{0} {1}"; //No token
        private const string m_QueryKeyTokenTemplate = "{0}-{1}";

        /// <summary>
        /// Queries server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The request name.</param>
        /// <param name="content">The request content.</param>
        /// <param name="executor">The response handler.</param>
        /// <returns>return token of the request</returns>
        public string Query<T>(string name, object content, Action<T> executor)
        {
            return Query<T>(name, content, new JsonExecutor<T>(executor));
        }

        /// <summary>
        /// Queries server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The request name.</param>
        /// <param name="content">The request content.</param>
        /// <param name="executor">The response handler.</param>
        /// <returns>return token of the request</returns>
        public string Query<T>(string name, object content, Action<string, T> executor)
        {
            return Query<T>(name, content, new JsonExecutorWithToken<T>(executor));
        }

        /// <summary>
        /// Queries server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The request name.</param>
        /// <param name="content">The request content.</param>
        /// <param name="executor">The response handler.</param>
        /// <returns></returns>
        public string Query<T>(string name, object content, Action<JsonWebSocket, T> executor)
        {
            return Query<T>(name, content, new JsonExecutorWithSender<T>(executor));
        }

        /// <summary>
        /// Queries server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The request name.</param>
        /// <param name="content">The request content.</param>
        /// <param name="executor">The response handler.</param>
        /// <returns></returns>
        public string Query<T>(string name, object content, Action<JsonWebSocket, string, T> executor)
        {
            return Query<T>(name, content, new JsonExecutorFull<T>(executor));
        }

        /// <summary>
        /// Queries the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="content">The content.</param>
        /// <param name="executor">The executor.</param>
        /// <param name="state">The callback state.</param>
        /// <returns></returns>
        public string Query<T>(string name, object content, Action<JsonWebSocket, T, object> executor, object state)
        {
            return Query<T>(name, content, new JsonExecutorWithSenderAndState<T>(executor, state));
        }

        string Query<T>(string name, object content, IJsonExecutor executor)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            int token = m_Random.Next(1000, 9999);

            RegisterExecutor<T>(name, token.ToString(), executor);

            if (content != null)
            {
                if (!content.GetType().IsPrimitive)
                    m_WebSocket.Send(string.Format(m_QueryTemplateA, name, token, SerializeObject(content)));
                else
                    m_WebSocket.Send(string.Format(m_QueryTemplateA, name, token, content));
            }
            else
                m_WebSocket.Send(string.Format(m_QueryTemplateB, name, token));

            return token.ToString();
        }

        private Dictionary<string, IJsonExecutor> m_ExecutorDict = new Dictionary<string, IJsonExecutor>(StringComparer.OrdinalIgnoreCase);

        void RegisterExecutor<T>(string name, string token, IJsonExecutor executor)
        {
            lock (m_ExecutorDict)
            {
                if (string.IsNullOrEmpty(token))
                    m_ExecutorDict.Add(name, executor);
                else
                    m_ExecutorDict.Add(string.Format(m_QueryKeyTokenTemplate, name, token), executor);
            }
        }

        IJsonExecutor GetExecutor(string name, string token)
        {
            string key = name;
            bool removeExecutor = false;

            if (!string.IsNullOrEmpty(token))
            {
                key = string.Format(m_QueryKeyTokenTemplate, name, token);
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
