using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SuperSocket;
using SuperSocket.Client;
using SuperSocket.ProtoBase;

namespace WebSocket4Net.Tests
{
    public interface IHostConfigurator
    {
        void Configure(HostBuilderContext context, IServiceCollection services);

        string WebSocketSchema { get; }

        bool IsSecure { get; }

        ListenOptions Listener { get; }

        WebSocket ConfigureClient(WebSocket client);
    }
}