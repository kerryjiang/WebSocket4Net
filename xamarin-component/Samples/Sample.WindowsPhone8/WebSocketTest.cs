using System;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using WebSocket4Net;

namespace Sample.iOSTest
{
    [TestClass]
    public class WebSocketTest
    {
        protected AutoResetEvent m_MessageReceiveEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_OpenedEvent = new AutoResetEvent(false);
        protected AutoResetEvent m_CloseEvent = new AutoResetEvent(false);
        protected string m_CurrentMessage = string.Empty;

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
        public void TestConnection()
        {
            var webSocketClient = new WebSocket("ws://echo.websocket.org");
            //webSocketClient.AllowUnstrustedCertificate = true;
            webSocketClient.Opened += new EventHandler(webSocketClient_Opened);
            webSocketClient.Closed += new EventHandler(webSocketClient_Closed);
            webSocketClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived);
            webSocketClient.Open();

            if (!m_OpenedEvent.WaitOne(5000))
            {
                Assert.Fail("Failed to Opened session ontime");
            }

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

                Assert.AreEqual(m_CurrentMessage, message);
            }

            webSocketClient.Close();

            if (!m_CloseEvent.WaitOne(5000))
            {
                Assert.Fail("Failed to close session ontime");
            }

            Assert.AreEqual(WebSocketState.Closed, webSocketClient.State);
        }
    }
}