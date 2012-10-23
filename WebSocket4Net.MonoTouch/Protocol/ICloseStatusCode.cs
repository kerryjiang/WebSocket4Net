using System;

namespace WebSocket4Net.Protocol
{
    public interface ICloseStatusCode
    {
        int ExtensionNotMatch { get; }
        int GoingAway { get; }
        int InvalidUTF8 { get; }
        int NormalClosure { get; }
        int NotAcceptableData { get; }
        int ProtocolError { get; }
        int TLSHandshakeFailure { get; }
        int TooLargeFrame { get; }
        int UnexpectedCondition { get; }
        int ViolatePolicy { get; }
        int NoStatusCode { get; }
    }
}
