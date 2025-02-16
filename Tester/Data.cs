using RabbitTester.Domain.Events;

namespace RabbitTester.Tester
{
    public class Data : Event
    {
        public int Id { get; set; }
    }
}
