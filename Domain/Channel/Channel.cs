using RabbitMQ.Client;
using RabbitTester.Domain.Configuration;

namespace RabbitTester.Domain.Channel
{
    public class Channel : IChannel
    {
        public HostConfiguration HostConfiguration { get; set; }
        public bool IsDefault { get; set; }
        public IModel Model { get; set; }
    }
}
