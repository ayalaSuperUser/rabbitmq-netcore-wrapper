using RabbitMQ.Client;
using RabbitTester.Domain.Common;
using RabbitTester.Domain.DTO;
using RabbitTester.Domain.Events;

namespace RabbitTester.Domain.Bus
{
    public interface IEventBus
    {
        Task Publish<T>(T @event, string queueName, QueueType queueType = RabbitMQConstants.QUEUE_TYPE, bool durable = true, IBasicProperties basicProperties = null, Dictionary<string, object> queueArgs = null) where T : Event;

        Task Publish<T>(PublishExchangeRequest<T> rabbitPublishExchangeRequest)
            where T : Event;

        ConsumerResponse Subscribe<T, TH>(string queueName, QueueType queueType = RabbitMQConstants.QUEUE_TYPE, bool autoAck = false, bool durable = true, bool requeue = false, Dictionary<string, object> queueArgs = null, ushort prefetchCount = RabbitMQConstants.PREFETCH_COUNT)
            where T : Event
            where TH : IEventHandler<T>;

        ConsumerResponse Subscribe<T, TH>(ConsumeExchangeRequest<T> rabbitConsumeExchangeRequest)
            where T : Event
            where TH : IEventHandler<T>;

        void Unsubscribe<TH>(string consumerTag);

        void Dispose();
    }
}
