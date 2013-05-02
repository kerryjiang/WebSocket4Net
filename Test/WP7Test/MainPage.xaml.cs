using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using WebSocket4Net;

namespace WP7Test
{
    public partial class MainPage : PhoneApplicationPage
    {
        private WebSocket m_WebSocket;

        private string m_CurrentSendMessage;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var url = "wss://localhost:2013/";

            m_WebSocket = new WebSocket(url);
            m_WebSocket.Opened += new EventHandler(m_WebSocket_Opened);
            m_WebSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(m_WebSocket_MessageReceived);
            m_WebSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(m_WebSocket_Error);
            m_WebSocket.Closed += new EventHandler(m_WebSocket_Closed);

            m_WebSocket.Open();
            AppendText("Connecting " + url + "...");
        }

        private void AppendText(string message)
        {
            Dispatcher.BeginInvoke(() =>
                {
                    OutputTextBox.Text = OutputTextBox.Text + "\r\n" + message;
                });
        }

        void m_WebSocket_Closed(object sender, EventArgs e)
        {
            AppendText("Closed");
        }

        void m_WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            AppendText("Error:" + e.Exception.Message);
        }

        void m_WebSocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            AppendText("Received:" + e.Message);

            if (e.Message.Equals(m_CurrentSendMessage))
                AppendText("Receive correct");
            else
                AppendText("Receive error");

            m_WebSocket.Close();
        }

        void m_WebSocket_Opened(object sender, EventArgs e)
        {
            AppendText("Opened");
            m_CurrentSendMessage = Guid.NewGuid().ToString();
            AppendText("Send");
            m_WebSocket.Send("ECHO " + m_CurrentSendMessage);
        }
    }
}