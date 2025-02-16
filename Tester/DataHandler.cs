using RabbitTester.Domain.Bus;

namespace RabbitTester.Tester
{
    public class DataHandler : IEventHandler<Data>
    {
        public Task Handle(Data @event)
        {
            return Task.CompletedTask;
        }
    }
}
