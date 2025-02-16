using RabbitMQ.Client;
using RabbitTester.Domain.Configuration;

namespace RabbitTester.Domain.Channel
{
    public interface IChannel
    {
        HostConfiguration HostConfiguration { get; set; }
        bool IsDefault { get; set; }
        IModel Model { get; set; }
    }
}
