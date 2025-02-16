namespace RabbitTester.Domain.Configuration
{
    public class RabbitMqConfigurations : IRabbitMqConfigurations
    {
        public List<RabbitMqConfiguration> Configurations { get; set; }
    }

    public class RabbitMqConfiguration : IRabbitMqConfiguration
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public HostConfiguration HostConfiguration { get; set; }
        public bool IsDefault { get; set; } = true;

    }

    public class HostConfiguration : IHostConfiguration
    {
        public string Key { get; set; }
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
    }
}
