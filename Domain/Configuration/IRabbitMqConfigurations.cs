namespace RabbitTester.Domain.Configuration
{
    public interface IRabbitMqConfigurations
    {
        List<RabbitMqConfiguration> Configurations { get; set; }
    }

    public interface IRabbitMqConfiguration
    {
        HostConfiguration HostConfiguration { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        bool IsDefault { get; set; }
    }

    public interface IHostConfiguration
    {
        string Key { get; set; }
        string HostName { get; set; }
        string VirtualHost { get; set; }
    }
}
