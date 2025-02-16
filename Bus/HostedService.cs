using RabbitMQ.Client;

namespace RabbitTester.Bus
{
    public class HostedService : IHostedService
    {
        private readonly IModel _channel;

        public HostedService(IModel channel)
        {
            _channel = channel;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Dispose();
            return Task.CompletedTask;
        }
    }
}
