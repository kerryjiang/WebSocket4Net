using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperSocket;
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

                var package = await websocket.ReceiveAsync(
                    handleControlPackage: true,
                    returnControlPackage: true);

                Assert.NotNull(package);
                
                var lastPingReceivedNow = websocket.PingPongStatus.LastPingReceived;

                Assert.NotEqual(lastPingReceived, lastPingReceivedNow);

                await websocket.CloseAsync();

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestIncorrectDNS(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var wrongHost = "localhost_x";

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{wrongHost}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                Assert.False(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestReconnect(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                Assert.True(await server.StartAsync());
                OutputHelper.WriteLine("Server started.");

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                for (var i = 0; i < 2000; i++)
                {
                    Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                    Assert.Equal(WebSocketState.Open, websocket.State);

                    await Task.WhenAny(websocket.CloseAsync(CloseReason.NormalClosure).AsTask(), Task.Delay(TimeSpan.FromSeconds(30)));
                    Assert.Equal(WebSocketState.Closed, websocket.State);

                    cancellationTokenSource.TryReset();
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestUnreachableReconnectA(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await server.StartAsync());

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                
                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);
                await Task.WhenAny(websocket.CloseAsync(CloseReason.NormalClosure).AsTask(), Task.Delay(TimeSpan.FromSeconds(30)));
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();

                cancellationTokenSource.TryReset();

                Assert.False(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Closed, websocket.State);

                Assert.True(await server.StartAsync());

                cancellationTokenSource.TryReset();

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);
                await Task.WhenAny(websocket.CloseAsync(CloseReason.NormalClosure).AsTask(), Task.Delay(TimeSpan.FromSeconds(30)));
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestUnreachableReconnectB(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                Assert.False(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Closed, websocket.State);

                Assert.True(await server.StartAsync());

                cancellationTokenSource.TryReset();

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);
                await Task.WhenAny(websocket.CloseAsync(CloseReason.NormalClosure).AsTask(), Task.Delay(TimeSpan.FromSeconds(30)));
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestCloseWebSocket(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(
                configurator: builder => {
                    builder.UseWebSocketMessageHandler(async (s, p) =>
                        {
                            if (p.Message == "QUIT")
                            {
                                await s.CloseAsync(CloseReason.NormalClosure);
                            }
                        });

                    return builder;
                },
                hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var manualResetEvent = new ManualResetEvent(false);

                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                websocket.Closed += (s, e) =>
                {
                    manualResetEvent.Set();
                };

                hostConfigurator.ConfigureClient(websocket);
                
                Assert.True(await server.StartAsync());

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);

                await websocket.SendAsync("QUIT");

                await Task.WhenAny(websocket.ReceiveAsync().AsTask(), Task.Delay(TimeSpan.FromSeconds(30)));

                Assert.True(manualResetEvent.WaitOne(TimeSpan.FromSeconds(5)), "The connection failed to close on time");
                Assert.Equal(WebSocketState.Closed, websocket.State);

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestSendMessage(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(
                configurator: builder =>
                {
                    builder.UseWebSocketMessageHandler(async (s, p) =>
                        {
                            if (p.Message.StartsWith("ECHO", StringComparison.OrdinalIgnoreCase))
                            {
                                await s.SendAsync(p.Message.Substring(5));
                            }
                        });

                    return builder;
                },
                hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await server.StartAsync());

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);

                var sb = new StringBuilder();

                for (int i = 0; i < 10; i++)
                {
                    sb.Append(Guid.NewGuid().ToString());
                }

                string messageSource = sb.ToString();

                var rd = new Random();

                for (int i = 0; i < 100; i++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                    string message = messageSource.Substring(startPos, endPos - startPos);

                    await websocket.SendAsync("ECHO " + message);

                    var receivedMessage = await websocket.ReceiveAsync();

                    Assert.NotNull(receivedMessage);
                    Assert.Equal(message, receivedMessage.Message);
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestSendData(Type hostConfiguratorType)
        {
            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            using (var server = CreateWebSocketSocketServerBuilder(
                configurator: builder =>
                {
                    builder.UseWebSocketMessageHandler(async (s, p) =>
                        {
                            await s.SendAsync(p.Data.ToArray());
                        });

                    return builder;
                },
                hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await server.StartAsync());

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);

                var sb = new StringBuilder();

                for (int i = 0; i < 10; i++)
                {
                    sb.Append(Guid.NewGuid().ToString());
                }

                string messageSource = sb.ToString();

                var rd = new Random();

                for (int i = 0; i < 100; i++)
                {
                    int startPos = rd.Next(0, messageSource.Length - 2);
                    int endPos = rd.Next(startPos + 1, messageSource.Length - 1);

                    string message = messageSource.Substring(startPos, endPos - startPos);

                    await websocket.SendAsync(Encoding.UTF8.GetBytes(message));

                    var receivedMessage = await websocket.ReceiveAsync();

                    Assert.NotNull(receivedMessage);
                    Assert.Equal(message, Encoding.UTF8.GetString(receivedMessage.Data));
                }

                await server.StopAsync();
            }
        }

        [Theory]
        [InlineData(typeof(RegularHostConfigurator))]
        [InlineData(typeof(SecureHostConfigurator))]
        public async Task TestConcurrentSend(Type hostConfiguratorType)
        {
            var lines = new string[100];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = Guid.NewGuid().ToString();
            }

            var messDict = new ConcurrentDictionary<string, string>(lines.Select(line => new KeyValuePair<string, string>(line, line)));

            var hostConfigurator = CreateObject<IHostConfigurator>(hostConfiguratorType);

            var allMessageReceivedEvent = new ManualResetEventSlim(false);

            using (var server = CreateWebSocketSocketServerBuilder(
                configurator: builder =>
                {
                    builder.UseWebSocketMessageHandler((s, p) =>
                        {
                            if (messDict.Remove(p.Message, out _))
                            {
                                if (messDict.Count == 0)
                                {
                                    allMessageReceivedEvent.Set();
                                }
                            }

                            return ValueTask.CompletedTask;
                        });

                    return builder;
                },
                hostConfigurator: hostConfigurator)
                .BuildAsServer())
            {
                var websocket = new WebSocket($"{hostConfigurator.WebSocketSchema}://{_loopbackIP}:{hostConfigurator.Listener.Port}");

                hostConfigurator.ConfigureClient(websocket);

                Assert.True(await server.StartAsync());

                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                Assert.True(await websocket.OpenAsync(cancellationTokenSource.Token), "Failed to connect");
                Assert.Equal(WebSocketState.Open, websocket.State);

                await AsyncParallel.ForEach(
                    lines,
                    async line =>
                    {
                        await websocket.SendAsync(line);
                    },
                    lines.Length);

                Assert.True(allMessageReceivedEvent.Wait(TimeSpan.FromSeconds(10)), "The server side didn't receive all messages in time.");
                await server.StopAsync();
            }
        }
    }
}
