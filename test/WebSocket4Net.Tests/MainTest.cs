using System;
using System.Threading.Tasks;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;
using Xunit;
using Xunit.Abstractions;

namespace WebSocket4Net.Tests
{
    public class MainTest : TestBase
    {
        public MainTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {

        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async ValueTask TestHandshake(Type hostConfiguratorType) 
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

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}" + path);

                hostConfigurator.ConfigureClient(websocket);

                Assert.Equal(WebSocketState.None, websocket.State);

                await websocket.OpenAsync();

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
        public async ValueTask TestEchoMessage(Type hostConfiguratorType, int repeat) 
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

                builder.UsePackageHandler(async (s, p) =>
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

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://localhost:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                await websocket.OpenAsync();

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
    }
}
