using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperWebSocket;
using SuperWebSocket.SubProtocol;
using WebSocket4Net;

namespace WebSocket4Net.Test
{
    [TestFixture]
    public class SecureWebSocketClientTestHybi00 : WebSocketClientTest
    {
        public SecureWebSocketClientTestHybi00()
            : base(WebSocketVersion.DraftHybi00, "Tls", "supersocket.pfx", "supersocket")
        {

        }

        protected override string Host
        {
            get { return "wss://127.0.0.1"; }
        }
    }

    [TestFixture]
    public class SecureWebSocketClientTestHybi10 : WebSocketClientTest
    {
        public SecureWebSocketClientTestHybi10()
            : base(WebSocketVersion.DraftHybi10, "Tls", "supersocket.pfx", "supersocket")
        {

        }

        protected override string Host
        {
            get { return "wss://127.0.0.1"; }
        }
    }

    [TestFixture]
    public class WebSocketClientTestHybi00 : WebSocketClientTest
    {
        public WebSocketClientTestHybi00()
            : base(WebSocketVersion.DraftHybi00)
        {

        }
    }

    [TestFixture]
    public class WebSocketClientTestHybi10 : WebSocketClientTest
    {
        public WebSocketClientTestHybi10()
            : base(WebSocketVersion.DraftHybi10)
        {

        }

        [Test]
        public void TestWebSocketOrg()
        {
            WebSocket webSocketClient = new WebSocket("ws://echo.websocket.org/");
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(5000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            for (var i = 0; i < 10; i++)
            {
                var message = Guid.NewGuid().ToString();

                webSocketClient.Send(message);

                if (!m_MessageReceiveEvent.WaitOne(5000))
                {
                    Assert.Fail("Failed to get echo messsage on time");
                    break;
                }

                Console.WriteLine("Received echo message: {0}", m_CurrentMessage);
                Assert.AreEqual(m_CurrentMessage, message);
            }

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(5000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }
    }

    public abstract class WebSocketClientTest
    {
        protected WebSocketServer m_WebSocketServer;
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);
        protected string m_CurrentMessage = string.Empty;

        private WebSocketVersion m_Version = WebSocketVersion.DraftHybi00;

        private string m_Security;
        private string m_CertificateFile;
        private string m_Password;

        protected virtual string Host
        {
            get { return "ws://127.0.0.1"; }
        }

        protected WebSocketClientTest(WebSocketVersion version)
            : this(version, string.Empty, string.Empty, string.Empty)
        {
            
        }

        protected WebSocketClientTest(WebSocketVersion version, string security, string certificateFile, string password)
        {
            m_Version = version;
            m_Security = security;
            m_CertificateFile = certificateFile;
            m_Password = password;
        }

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            LogUtil.Setup(new ConsoleLogger());

            m_WebSocketServer = new WebSocketServer(new BasicSubProtocol("Basic", new List<Assembly> { this.GetType().Assembly }));
            m_WebSocketServer.NewDataReceived += new SessionEventHandler<WebSocketSession, byte[]>(m_WebSocketServer_NewDataReceived);
            m_WebSocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 2012,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Async,
                Name = "SuperWebSocket Server",
                Security = m_Security,
                Certificate = new CertificateConfig { IsEnabled = true, FilePath = m_CertificateFile, Password = m_Password }
            }, SocketServerFactory.Instance);
        }

        void m_WebSocketServer_NewDataReceived(WebSocketSession session, byte[] e)
        {
            //Echo
            session.SendResponse(e);
        }

        [SetUp]
        public void StartServer()
        {
            m_WebSocketServer.Start();
        }

        [TearDown]
        public void StopServer()
        {
            m_WebSocketServer.Stop();
        }

        [Test, Repeat(5)]
        public void ConnectionTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(2000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        [Test]
        public void ReconnectTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);


            for (var i = 0; i <100; i++)
            {
                webSocketClient.Open();

                if (!m_OpenedEvent.WaitOne(5000))
                    Assert.Fail("Failed to Opened session ontime at round {0}", i);

                Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

                webSocketClient.Close();

                if (!m_CloseEvent.WaitOne(2000))
                    Assert.Fail("Failed to close session ontime");

                Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
            }
        }

        protected void webSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }

        [Test, Repeat(10)]
        public void CloseWebSocketTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(2000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            webSocketClient.Send("QUIT");

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        protected void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            m_CurrentMessage = e.Message;
            m_MessageReceiveEvent.Set();
        }

        protected void webSocketClient_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        protected void webSocketClient_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }

        [Test, Repeat(10)]
        public void SendMessageTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(2000))
                Assert.Fail("Failed to Opened session ontime");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + message);
                webSocketClient.Send("ECHO " + message);

                if (!m_MessageReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, m_CurrentMessage);
            }

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        [Test, Repeat(10)]
        public void SendDataTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);

            if (!webSocketClient.SupportBinary)
                return;

            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.DataReceived += new EventHandler<DataReceivedEventArgs>(webSocketClient_DataReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(2000))
                Assert.Fail("Failed to Opened session ontime");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 100; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("Client:" + message);
                var data = Encoding.UTF8.GetBytes(message);
                webSocketClient.Send(data, 0, data.Length);

                if (!m_MessageReceiveEvent.WaitOne(1000))
                    Assert.Fail("Cannot get response in time!");

                Assert.AreEqual(message, m_CurrentMessage);
            }

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        [Test]
        public void SendPingReactTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic", m_Version);
            webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(2000))
                Assert.Fail("Failed to Opened session ontime");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 10; i++)
            {
                sb.Append(Guid.NewGuid().ToString());
            }

            string messageSource = sb.ToString();

            Random rd = new Random();

            for (int i = 0; i < 10; i++)
            {
                int startPos = rd.Next(0, messageSource.Length - 2);
                int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                string message = messageSource.Substring(startPos, endPos - startPos);

                Console.WriteLine("PING:" + message);
                webSocketClient.Send("PING " + message);
            }

            Thread.Sleep(5000);

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");
        }

        protected void webSocketClient_DataReceived(object sender, DataReceivedEventArgs e)
        {
            m_CurrentMessage = Encoding.UTF8.GetString(e.Data);
            m_MessageReceiveEvent.Set();
        }
    }
}
