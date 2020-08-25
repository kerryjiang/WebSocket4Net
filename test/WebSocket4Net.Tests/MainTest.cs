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
        [Trait("Category", "TestHandshake")]
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
                async (s) =>
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

                await websocket.OpenAsync();

                //Assert.Equal(WebSocketState.Open, websocket.State);

                await Task.Delay(1 * 1000);
                // test path
                Assert.Equal(path, serverSessionPath);
                Assert.True(connected);

                await websocket.CloseAsync();

                Assert.NotNull(websocket.CloseStatus);
                Assert.Equal(CloseReason.NormalClosure, websocket.CloseStatus.Reason);

                await Task.Delay(1 * 1000);

                //Assert.Equal(WebSocketState.Closed, websocket.State);
                Assert.False(connected);

                await server.StopAsync();
            }
        }
    }
}
