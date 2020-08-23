using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
        private static readonly Encoding _asciiEncoding = Encoding.ASCII;
        private const string _magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public Uri Uri { get; private set; }

        private string _origin;

        private EndPoint _remoteEndPoint;

        private static readonly IPackageEncoder<WebSocketPackage> _packageEncoder = new WebSocketEncoder();

        private List<string> _subProtocols;

        public IReadOnlyList<string> SubProtocols
        {
            get { return _subProtocols; }
        }

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

            _origin = Uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);

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

        public void AddSubProtocol(string protocol)
        {
            var subProtocols = _subProtocols;

            if (subProtocols == null)
                subProtocols = _subProtocols = new List<string>();

            subProtocols.Add(protocol);
        }

        public async ValueTask<bool> OpenAsync(CancellationToken cancellationToken = default)
        {
            if (await this.ConnectAsync(_remoteEndPoint, cancellationToken))
                return false;

            var (key, acceptKey) = MakeSecureKey();
            await this.Channel.SendAsync((writer) => WriteHandshakeRequest(writer, key));

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

            var acceptKeyResponse = responseHeader.Items["Sec-WebSocket-Accept"];

            if (string.IsNullOrEmpty(acceptKeyResponse) || !acceptKeyResponse.Equals(acceptKey))
            {
                OnError($"The value of Sec-WebSocket-Accept is incorrect.", null);
                return false;
            }

            return true;
        }

        private string CalculateChallenge(string secKey, string magic)
        {
            return Convert.ToBase64String(SHA1.Create().ComputeHash(_asciiEncoding.GetBytes(secKey + magic)));
        }

        private (string, string) MakeSecureKey()
        {
            var secKey = Convert.ToBase64String(_asciiEncoding.GetBytes(Guid.NewGuid().ToString().Substring(0, 16)));
            return (secKey, CalculateChallenge(secKey, _magic));
        }

        private void WriteHandshakeRequest(PipeWriter writer, string secKey)
        {
            writer.Write($"GET {Uri.PathAndQuery} HTTP/1.1\r\n", _asciiEncoding);
            writer.Write($"{WebSocketConstant.Host}: {Uri.Host}\r\n", _asciiEncoding);
            writer.Write($"{WebSocketConstant.ResponseUpgradeLine}", _asciiEncoding);
            writer.Write($"{WebSocketConstant.ResponseConnectionLine}", _asciiEncoding);
            writer.Write($"{WebSocketConstant.SecWebSocketKey}: {secKey}\r\n", _asciiEncoding);
            writer.Write($"{WebSocketConstant.Origin}: {_origin}\r\n", _asciiEncoding);

            var subProtocols = _subProtocols;

            if (subProtocols != null && subProtocols.Count > 0)
            {
                var strSubProtocols = string.Join(", ", subProtocols);
                writer.Write($"{WebSocketConstant.SecWebSocketProtocol}: {strSubProtocols}\r\n", _asciiEncoding);
            }

            writer.Write($"{WebSocketConstant.SecWebSocketVersion}: 13\r\n\r\n", _asciiEncoding);
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
