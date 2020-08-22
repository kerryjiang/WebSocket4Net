using System;
using System.Buffers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Channel;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;

namespace WebSocket4Net
{
    public class WebSocket : EasyClient<WebSocketPackage>
    {
        public Uri Uri { get; private set; }

        private EndPoint _remoteEndPoint;

        private static readonly IPackageEncoder<WebSocketPackage> _packageEncoder = new WebSocketEncoder();

        public WebSocket(string url)
            : this(url, NullLogger.Instance)
        {

        }

        public WebSocket(string url, ILogger logger)
            : this(url, new ChannelOptions { Logger = logger })
        {

        }

        public WebSocket(string url, ChannelOptions channelOptions)
            : base(new HandshakePipelineFilter(), channelOptions)
        {
            Uri = new Uri(url);

            if ("ws".Equals(Uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                _remoteEndPoint = ResolveUri(Uri, 80);
            }
            else if ("wss".Equals(Uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                _remoteEndPoint = ResolveUri(Uri, 443);
            }
            else
            {
                throw new ArgumentException("Unexpected url schema.", nameof(url));
            }
        }

        private EndPoint ResolveUri(Uri uri, int defaultPort)
        {
            IPAddress ipAddress;

            EndPoint remoteEndPoint;

            var port = uri.Port;

            if (port <= 0)
                port = defaultPort;

            if (IPAddress.TryParse(uri.Host, out ipAddress))
                remoteEndPoint = new IPEndPoint(ipAddress, port);
            else
                remoteEndPoint = new DnsEndPoint(uri.Host, port);

            return remoteEndPoint;
        }


        private ReadOnlyMemory<byte> GetHandshakeRequest()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> OpenAsync(CancellationToken cancellationToken = default)
        {
            if (await this.ConnectAsync(_remoteEndPoint, cancellationToken))
                return false;

            await SendAsync(GetHandshakeRequest());

            var handshakeResponse = await ReceiveAsync();

            if (handshakeResponse == null)
                return false;

            var responseHeader = handshakeResponse.HttpHeader;

            // 101 switch
            if (responseHeader.StatusCode != "101")
            {
                OnError($"Unexpected response: {responseHeader.StatusCode} - {responseHeader.StatusDescription}", null);
                return false;
            }

            return true;
        }

        public new async ValueTask<WebSocketPackage> ReceiveAsync()
        {
            return await base.ReceiveAsync();
        }

        public async ValueTask SendAsync(string message)
        {
            var package = new WebSocketPackage();
            package.OpCode = OpCode.Text;
            package.Message = message;
            await SendAsync(_packageEncoder, package);
        }

        public new async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            var package = new WebSocketPackage();
            package.OpCode = OpCode.Binary;

            var sequenceElement = new SequenceSegment(data);
            package.Data = new ReadOnlySequence<byte>(sequenceElement, 0, sequenceElement, sequenceElement.Memory.Length);
            
            await SendAsync(_packageEncoder, package);
        }

        public ValueTask SendAsync(ref ReadOnlySequence<byte> sequence)
        {
            var package = new WebSocketPackage();
            package.OpCode = OpCode.Binary;
            package.Data = sequence;
            return SendAsync(_packageEncoder, package);
        }

        public new async ValueTask CloseAsync()
        {
            await CloseAsync(string.Empty);
        }

        public async ValueTask CloseAsync(string message)
        {
            var package = new WebSocketPackage();

            package.OpCode = OpCode.Close;
            package.Message = message;
            
            await SendAsync(_packageEncoder, package);

            var closeHandshakeResponse = await ReceiveAsync();

            if (closeHandshakeResponse.OpCode != OpCode.Close)
            {
                OnError($"Unexpected close package, OpCode: {closeHandshakeResponse.OpCode}", null);
                return;
            }

            await base.CloseAsync();
        }
    }
}
