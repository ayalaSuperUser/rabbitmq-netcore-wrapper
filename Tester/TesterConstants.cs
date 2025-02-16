using RabbitTester.Domain.DTO;

namespace RabbitTester.Tester
{
    public class TesterConstants
    {
        public const string ExchangeName = "tester";
        public const string QueueName1 = "tester.1";
        public const string MessagesCountKey = "MessagesCount";
        public const ExchangeType Exchange_Type = ExchangeType.Direct;
    }
}
