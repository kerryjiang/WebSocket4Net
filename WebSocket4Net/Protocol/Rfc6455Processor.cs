namespace WebSocket4Net.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/rfc6455
    /// </summary>
    internal class Rfc6455Processor : DraftHybi10Processor
    {
        public Rfc6455Processor()
            : base(WebSocketVersion.Rfc6455, new CloseStatusCodeRfc6455(), "Origin")
        {
        }
    }
}