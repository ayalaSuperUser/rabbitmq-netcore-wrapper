using RabbitMQ.Client;
using RabbitTester.Domain.Channel;
using RabbitTester.Domain.Configuration;

namespace RabbitTester.Bus
{
    public class ModelFactory : IDisposable
    {
        private List<IConnection> _connections = new List<IConnection>();
        public ModelFactory()
        {
        }

        public IChannel CreateChannel(RabbitMqConfiguration connectionConfiguration, HostConfiguration hostConfiguration)
        {
            var connection = GetConnection(connectionConfiguration);
            _connections.Add(connection);
            return new Channel()
            {
                HostConfiguration = hostConfiguration,
                IsDefault = connectionConfiguration.IsDefault,
                Model = connection.CreateModel(),
            };
        }

        public void Dispose()
        {
            foreach (var connection in _connections)
            {
                connection.Dispose();
            }
        }

        private IConnection GetConnection(IRabbitMqConfiguration rabbitMqConfiguration)
        {
            var connectionFactory = new ConnectionFactory
            {
                UserName = rabbitMqConfiguration.UserName,
                Password = rabbitMqConfiguration.Password,
                VirtualHost = string.IsNullOrEmpty(rabbitMqConfiguration.HostConfiguration.VirtualHost) ? "/" : rabbitMqConfiguration.HostConfiguration.VirtualHost,
                HostName = rabbitMqConfiguration.HostConfiguration.HostName,
                AutomaticRecoveryEnabled = true,
                DispatchConsumersAsync = true,
            };

            var endpoint = new List<AmqpTcpEndpoint>
            {
                new AmqpTcpEndpoint(rabbitMqConfiguration.HostConfiguration.HostName)
            };

            return connectionFactory.CreateConnection(endpoint);
        }


    }
}
