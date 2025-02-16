using RabbitTester.Domain.Events;

namespace RabbitTester.Domain.Bus
{
    public interface IEventHandler<in TEvent> : IEventHandler
       where TEvent : Event
    {
        Task Handle(TEvent @event);
    }

    public interface IEventHandler
    {
    }
}
