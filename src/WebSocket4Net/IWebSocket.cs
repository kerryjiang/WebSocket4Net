using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.Client;
using SuperSocket.WebSocket;

namespace WebSocket4Net
{
    public interface IWebSocket
    {
        ValueTask<bool> OpenAsync(CancellationToken cancellationToken = default);

        void StartReceive();

        event PackageHandler<WebSocketPackage> PackageHandler;

        ValueTask<WebSocketPackage> ReceiveAsync();

        ValueTask SendAsync(string message);

        ValueTask SendAsync(ReadOnlyMemory<byte> data);

        ValueTask SendAsync(ref ReadOnlySequence<byte> sequence);

        ValueTask CloseAsync(CloseReason closeReason, string message = null);

        event EventHandler Closed;
    }
}