using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SuperSocket.Connection;
using SuperSocket.Client;
using SuperSocket.ProtoBase;
using SuperSocket.WebSocket;
using CloseReason = SuperSocket.WebSocket.CloseReason;

namespace WebSocket4Net
{
    public class WebSocket : EasyClient<WebSocketPackage>, IWebSocket
    {
        private static readonly Encoding _asciiEncoding = Encoding.ASCII;
        private static readonly Encoding _utf8Encoding = new UTF8Encoding(false);
        private const string _magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public Uri Uri { get; private set; }

        public bool AutoPingEnabled { get; set; }

        public CloseStatus CloseStatus { get; private set; }

        public PingPongStatus PingPongStatus { get; private set; }

        private readonly string _origin;

        private readonly EndPoint _remoteEndPoint;

        private static readonly IPackageEncoder<WebSocketPackage> _packageEncoder = new WebSocketMaskedEncoder(ArrayPool<byte>.Shared, new int[]
            {
                1024,
                1024 * 4,
                1024 * 8,
                1024 * 16,
                1024 * 32,
                1024 * 64
            });

        private List<string> _subProtocols;

        public IReadOnlyList<string> SubProtocols => _subProtocols;

        private Dictionary<string, string> _headers;

        public Dictionary<string, string> Headers
        {
            get
            {
                return _headers ?? (_headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            }
        }

        public WebSocketState State { get; private set; } = WebSocketState.None;

        public WebSocket(string url)
            : this(url, NullLogger.Instance)
        {

        }

        public WebSocket(string url, ILogger logger)
            : this(url, new ConnectionOptions { Logger = logger })
        {

        }

        public WebSocket(string url, ConnectionOptions connectionOptions)
            : base(new HandshakePipelineFilter(), connectionOptions)
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

            PingPongStatus = new PingPongStatus();
        }

        private EndPoint ResolveUri(Uri uri, int defaultPort)
        {
            EndPoint remoteEndPoint;

            var port = uri.Port;

            if (port <= 0)
                port = defaultPort;

            if (IPAddress.TryParse(uri.Host, out IPAddress ipAddress))
                remoteEndPoint = new IPEndPoint(ipAddress, port);
            else
                remoteEndPoint = new DnsEndPoint(uri.Host, port);

            return remoteEndPoint;
        }

        public void AddSubProtocol(string protocol)
        {
            var subProtocols = _subProtocols;

            subProtocols ??= _subProtocols = new List<string>();
            subProtocols.Add(protocol);
        }

        protected override void SetupConnection(IConnection connection)
        {
            Closed += OnConnectionClosed;
            base.SetupConnection(connection);
        }

        public async ValueTask<bool> OpenAsync(CancellationToken cancellationToken = default)
        {
            State = WebSocketState.Connecting;

            if (!await ConnectAsync(_remoteEndPoint, cancellationToken))
            {
                State = WebSocketState.Closed;
                return false;
            }

            var (key, acceptKey) = MakeSecureKey();
            await Connection.SendAsync((writer) => WriteHandshakeRequest(writer, key));

            var handshakeResponse = await ReceiveAsync();

            if (handshakeResponse == null)
            {
                State = WebSocketState.Closed;
                return false;
            }

            var responseHeader = handshakeResponse.HttpHeader;

            // 101 switch
            if (responseHeader.StatusCode != "101")
            {
                OnError($"Unexpected response: {responseHeader.StatusCode} - {responseHeader.StatusDescription}");
                await base.CloseAsync(); // close the socket
                State = WebSocketState.Closed;
                return false;
            }

            var acceptKeyResponse = responseHeader.Items["Sec-WebSocket-Accept"];

            if (string.IsNullOrEmpty(acceptKeyResponse) || !acceptKeyResponse.Equals(acceptKey))
            {
                OnError($"The value of Sec-WebSocket-Accept is incorrect.");
                await base.CloseAsync(); // close the socket
                State = WebSocketState.Closed;
                return false;
            }

            State = WebSocketState.Open;

            if (AutoPingEnabled)
            {
                var autoPingTask = PingPongStatus.RunAutoPing(this, new AutoPingOptions(60 * 5, 5));
                _ = autoPingTask.ContinueWith(t => OnError("AutoPing failed", t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            }
            
            return true;
        }

        private string CalculateChallenge(string secKey, string magic)
            => Convert.ToBase64String(SHA1.Create().ComputeHash(_asciiEncoding.GetBytes(secKey + magic)));

        private (string, string) MakeSecureKey()
        {
            var secKey = Convert.ToBase64String(_asciiEncoding.GetBytes(Guid.NewGuid().ToString()[..16]));
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

            writer.Write($"{WebSocketConstant.SecWebSocketVersion}: 13\r\n", _asciiEncoding);
            
            if (_headers != null)
            {
                // Write extra headers
                foreach (var header in _headers)
                    writer.Write($"{header.Key}: {header.Value}\r\n", _asciiEncoding);
            }

            // Ensure end of the handshake request handshake
            writer.Write("\r\n", _asciiEncoding);
        }

        public new void StartReceive() => base.StartReceive();

        public new async ValueTask<WebSocketPackage> ReceiveAsync()
            => await ReceiveAsync(
                handleControlPackage: true,
                returnControlPackage: false);

        internal async ValueTask<WebSocketPackage> ReceiveAsync(bool handleControlPackage, bool returnControlPackage)
        {
            var package = await base.ReceiveAsync();

            if (package == null)
                return null;

            if (package.OpCode != OpCode.Binary && package.OpCode != OpCode.Text && package.OpCode != OpCode.Handshake)
            {
                if (handleControlPackage)
                {
                    await HandleControlPackage(package);
                }

                if (!returnControlPackage)
                {
                    return await ReceiveAsync(handleControlPackage, returnControlPackage);
                }
            }

            return package;
        }

        protected override async ValueTask OnPackageReceived(WebSocketPackage package)
        {
            if (package.OpCode != OpCode.Binary && package.OpCode != OpCode.Text)
            {
                await HandleControlPackage(package);
                return;
            }

            await base.OnPackageReceived(package);
        }

        private async ValueTask HandleControlPackage(WebSocketPackage package)
        {
            switch (package.OpCode)
            {
                case OpCode.Close:
                    await HandleCloseHandshake(package);
                    break;

                case OpCode.Ping:
                    PingPongStatus.OnPingReceived(package);
                    package.OpCode = OpCode.Pong;
                    await SendAsync(package);
                    break;

                case OpCode.Pong:
                    PingPongStatus.OnPongReceived(package);
                    break;
            }
        }

        public async ValueTask SendAsync(string message)
        {
            var package = new WebSocketPackage
            {
                OpCode = OpCode.Text,
                Message = message
            };
            await SendAsync(package);
        }

        internal async ValueTask SendAsync(WebSocketPackage package)
            => await SendAsync(_packageEncoder, package);

        public new async ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            var package = new WebSocketPackage
            {
                OpCode = OpCode.Binary
            };

            var sequenceElement = new SequenceSegment(data);
            package.Data = new ReadOnlySequence<byte>(sequenceElement, 0, sequenceElement, sequenceElement.Memory.Length);
            
            await SendAsync(_packageEncoder, package);
        }

        public ValueTask SendAsync(ref ReadOnlySequence<byte> sequence)
        {
            var package = new WebSocketPackage
            {
                OpCode = OpCode.Binary,
                Data = sequence
            };
            return SendAsync(_packageEncoder, package);
        }

        private byte[] GetBuffer(int size) => new byte[size];

        public override async ValueTask CloseAsync()
            => await CloseAsync(CloseReason.NormalClosure, string.Empty);

        public async ValueTask CloseAsync(CloseReason closeReason, string message = null)
        {
            var package = new WebSocketPackage
            {
                OpCode = OpCode.Close
            };

            var bufferSize = !string.IsNullOrEmpty(message) ? _utf8Encoding.GetMaxByteCount(message.Length) : 0;
            bufferSize += 2;

            var buffer = GetBuffer(bufferSize);
            var len = 2;

            BinaryPrimitives.WriteUInt16BigEndian(buffer, (ushort)package.OpCode);

            if (!string.IsNullOrEmpty(message))
            {
                len += _utf8Encoding.GetBytes(message, 0, message.Length, buffer, 2);
            }

            package.Data = new ReadOnlySequence<byte>(buffer, 0, len);

            CloseStatus = new CloseStatus
            {
                Reason = closeReason,
                ReasonText = message
            };
            
            await SendAsync(_packageEncoder, package);

            State = WebSocketState.CloseSent;

            var closeHandshakeResponse = await ReceiveAsync(
                handleControlPackage: false,
                returnControlPackage: true);

            if (closeHandshakeResponse.OpCode != OpCode.Close)
            {
                OnError($"Unexpected close package, OpCode: {closeHandshakeResponse.OpCode}");
            }
            else
            {
                await HandleCloseHandshake(closeHandshakeResponse);
            }

            await base.CloseAsync();

            State = WebSocketState.Closed;
        }

        private CloseStatus DecodeCloseStatus(WebSocketPackage closePackage)
        {
            var reader = new SequenceReader<byte>(closePackage.Data);
            reader.TryReadBigEndian(out ushort closeReason);            
            var reasonText = reader.ReadString(_utf8Encoding);

            return new CloseStatus
                {
                    Reason = (CloseReason)closeReason,
                    ReasonText = reasonText
                };
        }

        private async ValueTask HandleCloseHandshake(WebSocketPackage receivedClosePackage)
        {
            var closeStatusFromRemote = DecodeCloseStatus(receivedClosePackage);

            var closeStatus = CloseStatus;

            if (closeStatus == null)
            {
                State = WebSocketState.CloseReceived;
                closeStatusFromRemote.RemoteInitiated = true;
                CloseStatus = closeStatusFromRemote;
                // Send close pong message to server side.
                await SendAsync(receivedClosePackage);
            }
            else
            {
                if (closeStatus.Reason != closeStatusFromRemote.Reason)
                {
                    OnError("Unmatched CloseReason");
                    return;
                }
                
                // Received the close pong message from server,
                // so we can close the connection safely now
                await base.CloseAsync();
            }
        }

        private void OnConnectionClosed(object sender, EventArgs eventArgs) 
            => State = WebSocketState.Closed;
    }
}
