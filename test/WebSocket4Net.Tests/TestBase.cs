using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server.Abstractions.Host;
using SuperSocket.WebSocket;
using SuperSocket.WebSocket.Server;
using Xunit.Abstractions;

namespace WebSocket4Net.Tests
{
    public abstract class TestBase
    {
        protected readonly ITestOutputHelper OutputHelper;
        protected static readonly Encoding Utf8Encoding = new UTF8Encoding(false);
        protected readonly static int DefaultServerPort = 4040;

        protected IPEndPoint GetDefaultServerEndPoint()
        {
            return new IPEndPoint(IPAddress.Loopback, DefaultServerPort);
        }

        protected static ILoggerFactory DefaultLoggerFactory { get; } = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });

        protected TestBase(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        protected virtual void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {

        }

        protected ISuperSocketHostBuilder<WebSocketPackage> CreateWebSocketSocketServerBuilder(Func<ISuperSocketHostBuilder<WebSocketPackage>, ISuperSocketHostBuilder<WebSocketPackage>> configurator = null, IHostConfigurator hostConfigurator = null)
        {
            var hostBuilder = WebSocketHostBuilder.Create() as ISuperSocketHostBuilder<WebSocketPackage>;

            if (configurator != null)
                hostBuilder = configurator(hostBuilder);
                
            return Configure(hostBuilder, hostConfigurator) as ISuperSocketHostBuilder<WebSocketPackage>;
        }

        protected T CreateObject<T>(Type type)
        {
            return (T)ActivatorUtilities.CreateFactory(type, new Type[0]).Invoke(null, null);
        }

        protected IHostBuilder Configure(IHostBuilder hostBuilder, IHostConfigurator configurator = null)
        {
            var builder = hostBuilder.ConfigureAppConfiguration((hostCtx, configApp) =>
                {
                    configApp.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "serverOptions:name", "TestWebSocketServer" },
                        { "serverOptions:listeners:0:ip", "Any" },
                        { "serverOptions:listeners:0:backLog", "100" },
                        { "serverOptions:listeners:0:port", DefaultServerPort.ToString() }
                    });
                })
                .ConfigureLogging((hostCtx, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .ConfigureServices((hostCtx, services) =>
                {
                    ConfigureServices(hostCtx, services);
                });
            
            if (configurator != null)
            {
                builder = builder.ConfigureServices((ctx, services) =>
                {
                    configurator.Configure(ctx, services);
                });
            }

            return builder;
        }
    }
}
