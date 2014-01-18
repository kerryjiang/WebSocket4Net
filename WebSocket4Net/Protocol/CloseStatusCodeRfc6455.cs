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
