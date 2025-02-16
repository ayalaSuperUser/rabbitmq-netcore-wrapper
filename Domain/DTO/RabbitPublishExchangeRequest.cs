using RabbitMQ.Client;
using RabbitTester.Domain.Configuration;
using RabbitTester.Domain.Events;

namespace RabbitTester.Domain.DTO
{
    public class PublishExchangeRequest<T> : ExchangeRequest<T> where T : Event
    {
        public string ExchangeRoutingKey { get; set; } = "";
        public IBasicProperties BasicProperties { get; set; }
        IHostConfiguration HostConfiguration { get; set; }
    }
}
