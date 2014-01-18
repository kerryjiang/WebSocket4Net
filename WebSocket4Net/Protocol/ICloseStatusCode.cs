using System;

namespace WebSocket4Net.Protocol
{
    public interface ICloseStatusCode
    {
        short ExtensionNotMatch { get; }
        short GoingAway { get; }
        short InvalidUTF8 { get; }
        short NormalClosure { get; }
        short NotAcceptableData { get; }
        short ProtocolError { get; }
        short TLSHandshakeFailure { get; }
        short TooLargeFrame { get; }
        short UnexpectedCondition { get; }
        short ViolatePolicy { get; }
        short NoStatusCode { get; }
    }
}
