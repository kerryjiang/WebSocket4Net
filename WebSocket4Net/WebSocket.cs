using System;
using System.Collections;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using SuperSocket.ClientEngine;
using WebSocket4Net.Protocol;

namespace WebSocket4Net
{
    public partial class WebSocket : TcpClientSession
    {
        public WebSocketVersion Version { get; private set; }

        public DateTime LastActiveTime { get; internal set; }

        internal IProtocolProcessor ProtocolProcessor { get; private set; }

        public bool SupportBinary
        {
            get { return ProtocolProcessor.SupportBinary; }
        }

        internal Uri TargetUri { get; private set; }

        internal string SubProtocol { get; private set; }

        internal IDictionary<object, object> Items { get; private set; }

        internal List<KeyValuePair<string, string>> Cookies { get; private set; }

        public WebSocketState State { get; private set; }

        protected IClientCommandReader<WebSocketCommandInfo> CommandReader { get; private set; }

        private Dictionary<string, ICommand<WebSocket, WebSocketCommandInfo>> m_CommandDict
            = new Dictionary<string, ICommand<WebSocket, WebSocketCommandInfo>>(StringComparer.OrdinalIgnoreCase);

        public WebSocket(string uri)
            : this(uri, string.Empty)
        {

        }

        public WebSocket(string uri, WebSocketVersion version)
            : this(uri, string.Empty, null, version)
        {

        }

        public WebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, null, WebSocketVersion.DraftHybi10)
        {

        }

        public WebSocket(string uri, List<KeyValuePair<string, string>> cookies)
            : this(uri, string.Empty, cookies, WebSocketVersion.DraftHybi00)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies)
            : this(uri, subProtocol, cookies, WebSocketVersion.DraftHybi00)
        {

        }

        public WebSocket(string uri, string subProtocol, WebSocketVersion version)
            : this(uri, subProtocol, null, version)
        {

        }

        public WebSocket(string uri, string subProtocol, List<KeyValuePair<string, string>> cookies, WebSocketVersion version)
        {
            Version = version;
            ProtocolProcessor = GetProtocolProcessor(version);
            ProtocolProcessor.Initialize(this);
            CommandReader = ProtocolProcessor.CreateHandshakeReader();

            Cookies = cookies;

            var handshakeCmd = new Command.Handshake();
            m_CommandDict.Add(handshakeCmd.Name, handshakeCmd);
            var textCmd = new Command.Text();
            m_CommandDict.Add(textCmd.Name, textCmd);
            var dataCmd = new Command.Binary();
            m_CommandDict.Add(dataCmd.Name, dataCmd);
            var closeCmd = new Command.Close();
            m_CommandDict.Add(closeCmd.Name, closeCmd);
            var pongCmd = new Command.Pong();
            m_CommandDict.Add(pongCmd.Name, pongCmd);
            
            State = WebSocketState.None;

            TargetUri = new Uri(uri);

            SubProtocol = subProtocol;

            Items = new Dictionary<object, object>();

            if ("wss".Equals(TargetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("SuperWebSocket cannot support wss yet.", "uri");
            }

            if (!"ws".Equals(TargetUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid websocket address's schema.", "uri");
            }
        }

        public void Open()
        {
            State = WebSocketState.Connecting;
            
            IPAddress ipAddress;

            if (IPAddress.TryParse(TargetUri.Host, out ipAddress))
                RemoteEndPoint = new IPEndPoint(ipAddress, TargetUri.Port);
            else
                RemoteEndPoint = new DnsEndPoint(TargetUri.Host, TargetUri.Port);
            
            Connect();
        }

        private static IProtocolProcessor GetProtocolProcessor(WebSocketVersion version)
        {
            switch (version)
            {
                case(WebSocketVersion.DraftHybi00):
                    return new DraftHybi00Processor();
                case(WebSocketVersion.DraftHybi10):
                    return new DraftHybi10Processor();
            }

            throw new ArgumentException("Invalid websocket version");
        }

        protected override void OnConnected()
        {
            ProtocolProcessor.SendHandshake();
        }

        protected internal virtual void OnHandshaked()
        {
            State = WebSocketState.Open;

            if (m_Opened == null)
                return;

            m_Opened(this, EventArgs.Empty);
        }

        private EventHandler m_Opened;

        public event EventHandler Opened
        {
            add { m_Opened += value; }
            remove { m_Opened -= value; }
        }

        private EventHandler<MessageReceivedEventArgs> m_MessageReceived;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived
        {
            add { m_MessageReceived += value; }
            remove { m_MessageReceived -= value; }
        }

        internal void FireMessageReceived(string message)
        {
            if (m_MessageReceived == null)
                return;

            m_MessageReceived(this, new MessageReceivedEventArgs(message));
        }

        private EventHandler<DataReceivedEventArgs> m_DataReceived;

        public event EventHandler<DataReceivedEventArgs> DataReceived
        {
            add { m_DataReceived += value; }
            remove { m_DataReceived -= value; }
        }

        internal void FireDataReceived(byte[] data)
        {
            if (m_DataReceived == null)
                return;

            m_DataReceived(this, new DataReceivedEventArgs(data));
        }

        public void Send(string message)
        {
            ProtocolProcessor.SendMessage(message);
        }

        public new void Send(byte[] data, int offset, int length)
        {
            ProtocolProcessor.SendData(data, offset, length);
        }

        protected override void OnClosed()
        {
            var fireBaseClose = false;

            if (State == WebSocketState.Closing || State == WebSocketState.Open)
                fireBaseClose = true;

            State = WebSocketState.Closed;

            if (fireBaseClose)
                base.OnClosed();
        }

        public override void Close()
        {
            Close(string.Empty);
        }

        public void Close(string reason)
        {
            State = WebSocketState.Closing;
            ProtocolProcessor.SendCloseHandshake(reason);
            base.Close();
        }

        protected void ExecuteCommand(WebSocketCommandInfo commandInfo)
        {
            ICommand<WebSocket, WebSocketCommandInfo> command;

            if (m_CommandDict.TryGetValue(commandInfo.Key, out command))
            {
                command.ExecuteCommand(this, commandInfo);
            }
        }

        protected override void OnDataReceived(byte[] data, int offset, int length)
        {
            while (true)
            {
                int left;

                var commandInfo = CommandReader.GetCommandInfo(data, offset, length, out left);

                if (CommandReader.NextCommandReader != null)
                    CommandReader = CommandReader.NextCommandReader;

                if (commandInfo == null)
                    break;

                ExecuteCommand(commandInfo);

                if (left <= 0)
                    break;

                offset = offset + length - left;
                length = left;
            }
        }
    }
}
