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

        public short NormalClosure { get; private set; }
        public short GoingAway { get; private set; }
        public short ProtocolError { get; private set; }
        public short NotAcceptableData { get; private set; }
        public short TooLargeFrame { get; private set; }
        public short InvalidUTF8 { get; private set; }
        public short ViolatePolicy { get; private set; }
        public short ExtensionNotMatch { get; private set; }
        public short UnexpectedCondition { get; private set; }
        public short TLSHandshakeFailure { get; private set; }
        public short NoStatusCode { get; private set; }
    }
}
