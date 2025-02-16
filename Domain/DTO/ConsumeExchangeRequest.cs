using RabbitTester.Domain.Common;
using RabbitTester.Domain.Events;

namespace RabbitTester.Domain.DTO
{
    public class ConsumeExchangeRequest<T> : ExchangeRequest<T> where T : Event
    {
        public List<string> ExchangeRoutingKeys { get; set; } = new List<string>() { "" };
        /// <summary>
        /// Whether to acknowledge message automatically.
        /// Automatic acknowledgement does not work with the retry mechanism.
        /// </summary>
        public bool AutoAck { get; set; } = false;
        public string QueueName { get; set; } = string.Empty;
        [Obsolete("This field is depracated since classic queue is deprecated in rabbitMQ")]
        public QueueType QueueType { get; set; } = RabbitMQConstants.QUEUE_TYPE;
        [Obsolete("This field is depracated since classic queue is deprecated in rabbitMQ, quorum queue does not support non-durable queue")]
        public bool DurableQueue { get; set; } = true;
        /// <summary>
        /// Limitation number of unacknowledged messages on a channel when consuming.
        /// The default value is 200 messages.
        /// </summary>
        public ushort PrefetchCount { get; set; } = RabbitMQConstants.PREFETCH_COUNT;
        public Dictionary<string, object> QueueArgs { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// A retry message processing upon failure business logic
        /// </summary>
        //public RetryExchangeRequest RetryRequest { get; set; }
        /// <summary>
        /// A dead letter exchange that collect dropped messages from the original queue.
        /// </summary>
       // public DeadLetterExchangeRequest DLXRequest { get; set; }
    }
}
