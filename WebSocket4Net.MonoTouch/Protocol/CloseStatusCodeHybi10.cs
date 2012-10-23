using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol
{
    public class CloseStatusCodeHybi10 : ICloseStatusCode
    {
        public CloseStatusCodeHybi10()
        {
            NormalClosure = 1000;
            GoingAway = 1001;
            ProtocolError = 1002;
            NotAcceptableData = 1003;
            TooLargeFrame = 1004;
            InvalidUTF8 = 1007;
            ViolatePolicy = 1000;
            ExtensionNotMatch = 1000;
            UnexpectedCondition = 1000;
            TLSHandshakeFailure = 1000;
            NoStatusCode = 1005;
        }

        public int NormalClosure { get; private set; }
        public int GoingAway { get; private set; }
        public int ProtocolError { get; private set; }
        public int NotAcceptableData { get; private set; }
        public int TooLargeFrame { get; private set; }
        public int InvalidUTF8 { get; private set; }
        public int ViolatePolicy { get; private set; }
        public int ExtensionNotMatch { get; private set; }
        public int UnexpectedCondition { get; private set; }
        public int TLSHandshakeFailure { get; private set; }
        public int NoStatusCode { get; private set; }
    }
}
