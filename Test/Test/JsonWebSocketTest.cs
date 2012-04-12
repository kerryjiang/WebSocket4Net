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
using WebSocket4Net.Test.Json;

namespace WebSocket4Net.Test
{
    [TestFixture]
    public class JsonWebSocketTest
    {
        private WebSocketServer m_WebSocketServer;

        private AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        private AutoResetEvent m_CloseEvent = new AutoResetEvent(false);

        private Random m_Random = new Random();

        protected string Host
        {
            get { return "ws://127.0.0.1"; }
        }

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            ThreadPool.SetMinThreads(100, 100);

            LogUtil.Setup(new ConsoleLogger());

            m_WebSocketServer = new WebSocketServer(new BasicSubProtocol("Basic", new List<Assembly> { this.GetType().Assembly }));
            m_WebSocketServer.Setup(new RootConfig(), new ServerConfig
            {
                Port = 2012,
                Ip = "Any",
                MaxConnectionNumber = 100,
                Mode = SocketMode.Async,
                Name = "SuperWebSocket Server"
            }, SocketServerFactory.Instance);
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
        public void TestConnection()
        {
            var websocket = new JsonWebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic");
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Closed += new EventHandler(websocket_Closed);

            websocket.Open();

            if (!m_OpenedEvent.WaitOne(1000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, websocket.State);

            websocket.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, websocket.State);
        }

        [Test, Repeat(5)]
        public void TestOnHandler()
        {
            AddIn addInOut = null;

            AutoResetEvent responseEvent = new AutoResetEvent(false);

            var websocket = new JsonWebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic");
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Closed += new EventHandler(websocket_Closed);
            websocket.On<AddIn>("ECHOJSON", (e) =>
                {
                    addInOut = e;
                    responseEvent.Set();
                });

            websocket.Open();

            if (!m_OpenedEvent.WaitOne(1000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, websocket.State);


            for (var i = 0; i < 20; i++)
            {
                AddIn addIn = new AddIn { A = m_Random.Next(1, 9999), B = m_Random.Next(1, 9999) };

                websocket.Send("ECHOJSON", addIn);

                if (!responseEvent.WaitOne(1000))
                    Assert.Fail("Failed to get response ontime");

                Assert.AreEqual(addIn.A, addInOut.A);
                Assert.AreEqual(addIn.B, addInOut.B);
            }

            websocket.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, websocket.State);
        }


        [Test, Repeat(5)]
        public void TestQueryHandler()
        {
            AutoResetEvent responseEvent = new AutoResetEvent(false);

            var websocket = new JsonWebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic");
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Closed += new EventHandler(websocket_Closed);

            websocket.Open();

            if (!m_OpenedEvent.WaitOne(1000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, websocket.State);


            for (var i = 0; i < 20; i++)
            {
                AddOut addOut = null;

                AddIn addIn = new AddIn { A = m_Random.Next(1, 9999), B = m_Random.Next(1, 9999) };

                websocket.Query<AddOut>("ADD", addIn, (o) =>
                    {
                        addOut = o;
                        responseEvent.Set();
                    });

                if (!responseEvent.WaitOne(1000))
                    Assert.Fail("Failed to get response ontime");

                Assert.AreEqual(addIn.A + addIn.B, addOut.Result);
            }

            websocket.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, websocket.State);
        }


        [Test]
        public void TestLongQueryHandler()
        {
            AutoResetEvent responseEvent = new AutoResetEvent(false);

            var websocket = new JsonWebSocket(string.Format("{0}:{1}/websocket", Host, m_WebSocketServer.Config.Port), "basic");
            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Closed += new EventHandler(websocket_Closed);

            websocket.Open();

            if (!m_OpenedEvent.WaitOne(1000))
                Assert.Fail("Failed to Opened session ontime");

            Assert.AreEqual(WebSocketState.Open, websocket.State);


            for (var i = 0; i < 2; i++)
            {
                AddOut addOut = null;

                AddIn addIn = new AddIn { A = m_Random.Next(1, 9999), B = m_Random.Next(1, 9999) };

                websocket.Query<AddOut>("ADDX", addIn, (o) =>
                {
                    addOut = o;
                    responseEvent.Set();
                });

                if (!responseEvent.WaitOne(4000))
                    Assert.Fail("Failed to get response ontime");

                Assert.AreEqual(addIn.A + addIn.B, addOut.Result);
            }

            websocket.Close();

            if (!m_CloseEvent.WaitOne(1000))
                Assert.Fail("Failed to close session ontime");

            Assert.AreEqual(WebSocketState.Closed, websocket.State);
        }

        void websocket_Closed(object sender, EventArgs e)
        {
            m_CloseEvent.Set();
        }

        void websocket_Opened(object sender, EventArgs e)
        {
            m_OpenedEvent.Set();
        }
    }
}
