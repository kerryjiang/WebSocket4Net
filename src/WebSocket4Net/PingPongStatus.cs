using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.WebSocket;

namespace WebSocket4Net
{
    public class PingPongStatus
    {
        private TaskCompletionSource<WebSocketPackage> _pongReceivedTaskSource;

        public DateTimeOffset LastPingReceived  { get; internal set; }

        public DateTimeOffset LastPongReceived  { get; internal set; }

        internal PingPongStatus()
        {
        }

        internal async Task RunAutoPing(WebSocket webSocket, AutoPingOptions options)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var autoPingInterval = Math.Max(options.AutoPingInterval, 60) * 1000;

                await Task.Delay(autoPingInterval);

                _pongReceivedTaskSource = new TaskCompletionSource<WebSocketPackage>();

                await webSocket.SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Ping
                });

                var pongExpectAfterPing = Math.Max(options.ExpectedPongDelay, autoPingInterval * 3);
                var task = await Task.WhenAny(_pongReceivedTaskSource.Task, Task.Delay(pongExpectAfterPing));

                if (task is Task<WebSocketPackage>)
                {
                    continue;
                }

                // Pong doesn't arrive on time
                await webSocket.CloseAsync(CloseReason.UnexpectedCondition, "Pong is not received on time.");
                break;
            }
        }

        internal void OnPongReceived(WebSocketPackage pong)
        {
            LastPongReceived = DateTimeOffset.Now;
            _pongReceivedTaskSource.SetResult(pong);
        }

        internal void OnPingReceived(WebSocketPackage ping)
        {
            LastPingReceived = DateTimeOffset.Now;
        }
    }
}