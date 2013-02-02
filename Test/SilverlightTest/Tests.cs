using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#if WINDOWS_PHONE
    using Microsoft.Phone.Testing;
#else
    using Microsoft.Silverlight.Testing;
#endif

using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSocket4Net;
using System.Threading;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.Automation;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;

namespace SilverlightTest
{
    //[TestClass]
    public class SecureWebSocketClientTestHybi00 : WebSocketClientTest
    {
        public SecureWebSocketClientTestHybi00()
            : base(WebSocketVersion.DraftHybi00)
        {

        }

        protected override string Host
        {
            get { return "wss://127.0.0.1"; }
        }
    }

    [TestClass]
    public class SecureWebSocketClientTestHybi10 : WebSocketClientTest
    {
        public SecureWebSocketClientTestHybi10()
            : base(WebSocketVersion.DraftHybi10)
        {

        }

        protected override string Host
        {
            get { return "wss://127.0.0.1"; }
        }
    }

    //[TestClass]
    public class WebSocketClientTestHybi00 : WebSocketClientTest
    {
        public WebSocketClientTestHybi00()
            : base(WebSocketVersion.DraftHybi00)
        {

        }
    }

    //[TestClass]
    public class WebSocketClientTestHybi10 : WebSocketClientTest
    {
        public WebSocketClientTestHybi10()
            : base(WebSocketVersion.DraftHybi10)
        {

        }

        [TestMethod]
        public void TestWebSocketOrg()
        {
            WebSocket webSocketClient = new WebSocket("ws://echo.websocket.org/");
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
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
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);
        protected string m_CurrentMessage = string.Empty;

        private WebSocketVersion m_Version = WebSocketVersion.DraftHybi00;

        protected virtual string Host
        {
            get { return "ws://127.0.0.1"; }
        }

        protected virtual int Port
        {
            get { return 2014; }
        }

        protected WebSocketClientTest(WebSocketVersion version)
        {
            m_Version = version;
        }

        
        [ClassInitialize]
        public void StartServer()
        {
            //Console.WriteLine(Environment.CurrentDirectory);
            //dynamic cmd = AutomationFactory.CreateObject("WScript.Shell");
            //cmd.Run(@"..\..\..\bin\debug\TestServer.exe", 1, true);
        }

        [ClassCleanup]
        public void StopServer()
        {
            //dynamic cmd = AutomationFactory.CreateObject("WScript.Shell");
            //cmd.Run(@"..\..\..\bin\debug\TestServer.exe stop", 1, true);
        }

        [TestMethod]
        public void ConnectionTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            //webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne())
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        [TestMethod]
        public void TestNetworkStream()
        {
            EndPoint serverAddress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2021);

            var resetEvent = new AutoResetEvent(false);

            var args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = serverAddress;
            args.Completed += (sender, e) =>
            {
                resetEvent.Set();
            };

            Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, args);

            resetEvent.WaitOne();

            var encoding = new UTF8Encoding();

            using (Socket socket = args.ConnectSocket)
            {
                var socketStream = new SslStream(new NetworkStream(socket));
                socketStream.BeginAuthenticateAsClient("localhost", new AsyncCallback(r =>
                    {
                        resetEvent.Set();
                    }), null);

                resetEvent.WaitOne();

                using (var reader = new StreamReader(socketStream, encoding, true))
                using (var writer = new StreamWriter(socketStream, encoding, 1024 * 8))
                {
                    string welcomeString = reader.ReadLine();

                    Console.WriteLine("Welcome: " + welcomeString);

                    char[] chars = new char[] { 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H' };

                    Random rd = new Random(1);

                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < 50; i++)
                    {
                        sb.Append(chars[rd.Next(0, chars.Length - 1)]);
                        string command = sb.ToString();
                        writer.WriteLine("ECHO " + command);
                        writer.Flush();
                        string echoMessage = reader.ReadLine();
                        Console.WriteLine("C:" + echoMessage);
                        Assert.AreEqual(command, echoMessage);
                    }
                }
            }
        }

        //[TestMethod]
        public void IncorrectDNSTest()
        {
            //var autoReset = new AutoResetEvent(false);
            //var remoteEndPoint = new DnsEndPoint("localhostx", 2022);

            //var e = new SocketAsyncEventArgs();

            //e.RemoteEndPoint = remoteEndPoint;
            //e.Completed += (x, y) =>
            //    {
            //        autoReset.Set();
            //    };

            //Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp, e);
            //autoReset.WaitOne();
            //Assert.IsNull(e.ConnectSocket);

            //WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", "ws://localhostx", Port), subProtocol: "basic", version: m_Version);
            //webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            ////webSocketClient.AllowUnstrustedCertificate = true;
            //webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            //webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            //webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            //webSocketClient.Open();

            //if (!m_OpenedEvent.WaitOne(10000))
            //    Assert.Fail("Failed to Opened session ontime");

            //Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

            //webSocketClient.Close();

            //if (!m_CloseEvent.WaitOne(1000))
            //    Assert.Fail("Failed to close session ontime");

            //Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }

        //[TestMethod]
        public void ReconnectTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
            //webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);

            for (var i = 0; i < 2000; i++)
            {
                webSocketClient.Open();

                if (!m_OpenedEvent.WaitOne(5000))
                    Assert.Fail("Failed to Opened session ontime at round {0}", i);

                Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

                webSocketClient.Close();

                if (!m_CloseEvent.WaitOne(5000))
                    Assert.Fail("Failed to close session ontime");

                Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
            }
        }

        //[TestMethod]
        //public void UnreachableReconnectTestA()
        //{
        //    WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
        //    webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
        //    webSocketClient.Error += (s, e) => { m_OpenedEvent.Set(); };
        //    //webSocketClient.AllowUnstrustedCertificate = true;
        //    webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
        //    webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
        //    webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);

        //    webSocketClient.Open();

        //    if (!m_OpenedEvent.WaitOne(5000))
        //        Assert.Fail("Failed to Opened session ontime");

        //    Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

        //    webSocketClient.Close();

        //    if (!m_CloseEvent.WaitOne(2000))
        //        Assert.Fail("Failed to close session ontime");

        //    Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);

        //    StopServer();

        //    webSocketClient.Open();

        //    m_OpenedEvent.WaitOne();

        //    Assert.AreEqual(WebSocketState.None, webSocketClient.State);

        //    StartServer();

        //    webSocketClient.Open();

        //    if (!m_OpenedEvent.WaitOne(5000))
        //        Assert.Fail("Failed to Opened session ontime");

        //    Assert.AreEqual(WebSocketState.Open, webSocketClient.State);
        //}

        //[TestMethod]
        //public void UnreachableReconnectTestB()
        //{
        //    StopServer();

        //    WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
        //    webSocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(webSocketClient_Error);
        //    webSocketClient.Error += (s, e) => { m_OpenedEvent.Set(); };
        //    //webSocketClient.AllowUnstrustedCertificate = true;
        //    webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
        //    webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
        //    webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);

        //    webSocketClient.Open();
        //    m_OpenedEvent.WaitOne();
        //    Assert.AreEqual(WebSocketState.None, webSocketClient.State);

        //    StartServer();

        //    webSocketClient.Open();
        //    if (!m_OpenedEvent.WaitOne(5000))
        //        Assert.Fail("Failed to Opened session ontime");

        //    Assert.AreEqual(WebSocketState.Open, webSocketClient.State);

        //    webSocketClient.Close();

        //    if (!m_CloseEvent.WaitOne(2000))
        //        Assert.Fail("Failed to close session ontime");

        //    Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        //}

        protected void webSocketClient_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.GetType() + ":" + e.Exception.Message + Environment.NewLine + e.Exception.StackTrace);

            if (e.Exception.InnerException != null)
            {
                Console.WriteLine(e.Exception.InnerException.GetType());
            }
        }

        [TestMethod]
        public void CloseWebSocketTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            //webSocketClient.AllowUnstrustedCertificate = true;
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

        [TestMethod]
        public void SendMessageTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            //webSocketClient.AllowUnstrustedCertificate = true;
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

        [TestMethod]
        public void SendDataTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);

            if (!webSocketClient.SupportBinary)
                return;

            //webSocketClient.AllowUnstrustedCertificate = true;
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

        [TestMethod]
        public void SendPingReactTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            //webSocketClient.AllowUnstrustedCertificate = true;
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


        [TestMethod]
        public void ConcurrentSendTest()
        {
            WebSocket webSocketClient = new WebSocket(string.Format("{0}:{1}/websocket", Host, Port), subProtocol: "basic", version: m_Version);
            //webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);

            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(5000))
                Assert.Fail("Failed to Opened session ontime");

            string[] lines = new string[100];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Guid.NewGuid().ToString();
            }

            var messDict = lines.ToDictionary(l => l);

            webSocketClient.MessageReceived += (s, m) =>
            {
                messDict.Remove(m.Message);
                Console.WriteLine("R: {0}", m.Message);
            };

            for (var i = 0; i < lines.Length; i++)
            {
                ThreadPool.QueueUserWorkItem((w) =>
                    {
                        webSocketClient.Send("ECHO " + lines[(int)w]);
                    }, i);
            }

            int waitRound = 0;

            while (waitRound < 10)
            {
                if (messDict.Count <= 0)
                    break;

                Thread.Sleep(500);
                waitRound++;
            }

            if (messDict.Count > 0)
            {
                Assert.Fail("Failed to receive message on time.");
            }

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