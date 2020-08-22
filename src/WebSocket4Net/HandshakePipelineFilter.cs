
using System.Collections.Specialized;
using SuperSocket.WebSocket;

namespace WebSocket4Net
{
    internal class HandshakePipelineFilter : WebSocketPipelineFilter
    {
        protected override HttpHeader CreateHttpHeader(string verbItem1, string verbItem2, string verbItem3, NameValueCollection items)
        {
            return HttpHeader.CreateForResponse(verbItem1, verbItem2, verbItem3, items);
        }
    }
}