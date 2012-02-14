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
using System.Windows.Browser;

namespace WebSocket4Net.JsBridge
{
    [ScriptableType]
    public class MessageEventArgs : EventArgs
    {
        [ScriptableMember(ScriptAlias = "data")]
        public string Data { get; set; }
    }
}
