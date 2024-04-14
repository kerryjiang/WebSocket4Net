using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.Server.Abstractions;

namespace WebSocket4Net.Tests
{
    public class SecureHostConfigurator : IHostConfigurator
    {
        public string WebSocketSchema => "wss";

        public bool IsSecure => true;

        public ListenOptions Listener { get; private set; }

        public void Configure(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ServerOptions>((options) =>
                {
                    var listener = options.Listeners[0];
                    listener.Security = GetServerEnabledSslProtocols();
                    listener.CertificateOptions = new CertificateOptions
                    {
                        FilePath = "supersocket.pfx",
                        Password = "supersocket"
                    };
                    Listener = listener;
                });
        }

        protected virtual SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12;
        }

        protected virtual SslProtocols GetClientEnabledSslProtocols()
        {
            return SslProtocols.Tls13 | SslProtocols.Tls12;
        }

        public WebSocket ConfigureClient(WebSocket client)
        {
            client.Security = new SecurityOptions
            {
                TargetHost = "supersocket",
                EnabledSslProtocols = GetClientEnabledSslProtocols(),
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            
            return client;
        }
    }

    public class TLS13OnlySecureHostConfigurator : SecureHostConfigurator
    {
        protected override SslProtocols GetServerEnabledSslProtocols()
        {
            return SslProtocols.Tls13;
        }

        protected override SslProtocols GetClientEnabledSslProtocols()
        {
            return SslProtocols.Tls13;
        }
    }
}