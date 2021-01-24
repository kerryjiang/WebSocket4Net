using System;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket.WebSocket;

namespace WebSocket4Net
{
    public class PingPongStatus
    {
        public bool AutoPingEnabled { get; private set; }

        /// <summary>
        /// The interval of the auto ping
        /// </summary>
        /// <value>in seconds</value>
        public int AutoPingInterval { get; private set; }

        /// <summary>
        /// How long we expect receive pong after ping is sent
        /// </summary>
        /// <value>in seconds</value>
        public int ExpectedPongDelay { get; private set; }

        private TaskCompletionSource<WebSocketPackage> _pongReceivedTaskSource;

        public DateTimeOffset LastPingReceived  { get; internal set; }

        public DateTimeOffset LastPongReceived  { get; internal set; }

        internal WebSocket WebSocket { get; set; }

        internal PingPongStatus(AutoPingOptions options)
        {
            if (options != null)
            {
                AutoPingEnabled = true;
                AutoPingInterval = options.AutoPingInterval;
                ExpectedPongDelay = options.ExpectedPongDelay;
            }
        }

        internal void Start()
        {
            RunAutoPing();
        }

        private async void RunAutoPing()
        {
            while (AutoPingEnabled)
            {
                var autoPingInterval = Math.Max(AutoPingInterval, 60) * 1000;

                await Task.Delay(autoPingInterval);

                _pongReceivedTaskSource = new TaskCompletionSource<WebSocketPackage>();

                await WebSocket.SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Ping
                });

                var pongExpectAfterPing = Math.Max(ExpectedPongDelay, autoPingInterval * 3);
                var task = await Task.WhenAny(_pongReceivedTaskSource.Task, Task.Delay(pongExpectAfterPing));

                if (task is Task<WebSocketPackage> pongTask)
                {
                    LastPongReceived = DateTimeOffset.Now;
                    continue;
                }

                // Pong doesn't arrive on time
                await WebSocket.CloseAsync(CloseReason.UnexpectedCondition, "Pong is not received on time.");
            }
        }

        internal void Stop()
        {
            AutoPingEnabled = false;
        }

        internal ValueTask OnPongReceived(WebSocketPackage pong)
        {
            _pongReceivedTaskSource.SetResult(pong);
            return new ValueTask();
        }

        internal async ValueTask OnPingReceived(WebSocketPackage ping)
        {
            ping.OpCode = OpCode.Pong;
            await WebSocket.SendAsync(ping);
            LastPingReceived = DateTimeOffset.Now;
        }
    }
}