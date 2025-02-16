using RabbitTester.Domain.Events;

namespace RabbitTester.Domain.DTO
{
    public abstract class ExchangeRequest
    {
        public string ExchangeName { get; set; }
        public ExchangeType ExchangeType { get; set; } = ExchangeType.Fanout;
        public bool Durable { get; set; }
    }

    public abstract class ExchangeRequest<T> : ExchangeRequest where T : Event
    {
        public string DelayExchangeName { get { return $"{ExchangeName}.delay"; } }
        public T @event { get; set; }
    }

    public enum ExchangeType
    {
        Fanout,
        Direct,
        Topic,
        Headers
    }

    public enum QueueType
    {
        [Obsolete("Classic queue is deprecated in rabbitMQ, please use quorum queue instead")]
        Classic,
        Quorum
    }
}
