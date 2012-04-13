using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol
{
    public class CloseStatusCodeRfc6455 : ICloseStatusCode
    {
        public CloseStatusCodeRfc6455()
        {
            NormalClosure = 1000;
            GoingAway = 1001;
            ProtocolError = 1002;
            NotAcceptableData = 1003;
            TooLargeFrame = 1009;
            InvalidUTF8 = 1007;
            ViolatePolicy = 1008;
            ExtensionNotMatch = 1010;
            UnexpectedCondition = 1011;
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
