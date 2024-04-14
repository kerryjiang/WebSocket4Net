using System;
using System.Buffers;
using System.Net;
using System.Threading.Tasks;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace WebSocket4Net.Tests
{
    public class MainTest : TestBase
    {
        private string _loopbackIP;
        public MainTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _loopbackIP = IPAddress.Loopback.ToString();
        }


        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestHandshake(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;
            var connected = false;

            using (var server = CreateWebSocketSocketServerBuilder(builder =>
            {
                builder.UseSessionHandler(async (s) =>
                {
                    serverSessionPath = (s as WebSocketSession).Path;
                    connected = true;
                    await Task.CompletedTask;
                },
                async (s, e) =>
                {
                    connected = false;
                    await Task.CompletedTask;
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var path = "/app/talk";
                var url = $"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}" + path;

                var websocket = new WebSocket(url);

                hostConfigurator.ConfigureClient(websocket);

                Assert.Equal(WebSocketState.None, websocket.State);

                Assert.True(await websocket.OpenAsync(), "Failed to connect");

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);
                // test path
                Assert.Equal(path, serverSessionPath);
                Assert.True(connected);

                await websocket.CloseAsync();

                Assert.NotNull(websocket.CloseStatus);
                Assert.Equal(CloseReason.NormalClosure, websocket.CloseStatus.Reason);

                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);
                Assert.False(connected);

                await server.StopAsync();
            }
        }


        [Theory]
        [InlineData(typeof(RegularHostConfigurator), 10)]
        [InlineData(typeof(SecureHostConfigurator), 10)]
        public async Task TestEchoMessage(Type hostConfiguratorType, int repeat) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var serverSessionPath = string.Empty;
            var connected = false;

            using (var server = CreateWebSocketSocketServerBuilder(builder =>
            {
                builder.UseSessionHandler(async (s) =>
                {
                    connected = true;
                    await Task.CompletedTask;
                },
                async (s, e) =>
                {
                    connected = false;
                    await Task.CompletedTask;
                });

                builder.UseWebSocketMessageHandler(async (s, p) =>
                {
                    var session = s as WebSocketSession;
                    await session.SendAsync(p.Message);
                });

                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await websocket.OpenAsync(), "Failed to connect");

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);                
                Assert.True(connected);

                for (var i = 0; i < repeat; i++)
                {
                    var text = Guid.NewGuid().ToString();
                    await websocket.SendAsync(text);
                    var receivedText = (await websocket.ReceiveAsync()).Message;
                    Assert.Equal(text, receivedText);
                }

                await websocket.CloseAsync();

                Assert.NotNull(websocket.CloseStatus);
                Assert.Equal(CloseReason.NormalClosure, websocket.CloseStatus.Reason);

                await Task.Delay(1 * 1000);

                Assert.Equal(WebSocketState.Closed, websocket.State);
                Assert.False(connected);

                await server.StopAsync();
            }
        }

        
        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestPingFromServer(Type hostConfiguratorType) 
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            WebSocketSession session = null;

            using (var server = CreateWebSocketSocketServerBuilder(builder =>
            {
                builder.UseSessionHandler(async (s) =>
                {
                    session = s as WebSocketSession;
                    await Task.CompletedTask;
                });
                return builder;
            }, hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await websocket.OpenAsync(), "Failed to connect");

                var lastPingReceived = websocket.PingPongStatus.LastPingReceived;

                Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);

                Assert.NotNull(session);                

                // send ping from server
                await session.SendAsync(new WebSocketPackage
                {
                    OpCode = OpCode.Ping,
                    Data = new ReadOnlySequence<byte>(Utf8Encoding.GetBytes("Hello"))
                });
                
                await Task.Delay(1 * 1000);
                var lastPingReceivedNow = websocket.PingPongStatus.LastPingReceived;

                Assert.NotEqual(lastPingReceived, lastPingReceivedNow);

                await websocket.CloseAsync();

                await server.StopAsync();
            }
        }
    }
}
