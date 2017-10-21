using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebSocket4Net;
using Xunit;

namespace Tests
{
    public class Tests
    {

        private IWebHost StartWebSocketServer(Func<HttpContext, Task> app)
        {
            Action<IApplicationBuilder> startup = builder =>
            {
                builder.Use(async (ct, next) =>
                {
                    try
                    {
                        // Kestrel does not return proper error responses:
                        // https://github.com/aspnet/KestrelHttpServer/issues/43
                        await next();
                    }
                    catch (Exception ex)
                    {
                        if (ct.Response.HasStarted)
                        {
                            throw;
                        }

                        ct.Response.StatusCode = 500;
                        ct.Response.Headers.Clear();
                        await ct.Response.WriteAsync(ex.ToString());
                    }
                });

                builder.UseWebSockets();
                builder.Run(c => app(c));
            };

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection();
            var config = configBuilder.Build();
            config["server.urls"] = "http://localhost:54321";

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .Configure(startup)
                .Build();

            host.Start();

            return host;
        }

        private void StopWebSocketServer()
        {

        }

        [Fact]
        public async Task TestConnection() 
        {
            using (var server = StartWebSocketServer(async context =>
            {
                Assert.True(context.WebSockets.IsWebSocketRequest);
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Assert.NotNull(webSocket);
                Assert.Equal(System.Net.WebSockets.WebSocketState.Open, webSocket.State);
            }))
            {
                using (var client = new WebSocket4Net.WebSocket("ws://localhost:54321/"))
                {        
                    await client.OpenAsync();
                    Assert.Equal(WebSocket4Net.WebSocketState.Open, client.State);
                    await client.CloseAsync();
                    Assert.Equal(WebSocket4Net.WebSocketState.Closed, client.State);
                }
            }
        }
    }
}
