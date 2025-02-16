using RabbitTester.Domain.DTO;

namespace RabbitTester.Domain.Common
{
    public static class RabbitMQConstants
    {
        public const ushort PREFETCH_COUNT = 25;
        public const QueueType QUEUE_TYPE = QueueType.Quorum;
        public const int QUEUE_DELIVERY_COUNT = 10;
    }
}