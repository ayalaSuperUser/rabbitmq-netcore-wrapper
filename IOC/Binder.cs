using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RabbitTester.Bus;
using RabbitTester.Domain.Bus;
using RabbitTester.Domain.Channel;
using RabbitTester.Domain.Configuration;

namespace RabbitTester.IOC
{
    public static class Binder
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            services.AddRabbitMqConfigurations();

            services.AddHostConfigurations();

            services.AddModelFactory();

            services.AddDefaultChannel();

            services.AddKeyedChannels();

            services.AddDefaultEventBus();

            services.AddKeyedEventBuses();

            services.AddHostedService<HostedService>();

            return services;
        }

        private static IServiceCollection AddRabbitMqConfigurations(this IServiceCollection services)
        {
            services.TryAddSingleton<IRabbitMqConfigurations>(sp => sp.GetRequiredService<IOptions<RabbitMqConfigurations>>().Value);
            return services;
        }

        private static IServiceCollection AddHostConfigurations(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var rabbitMqConfigurations = provider.GetRequiredService<IRabbitMqConfigurations>();

            rabbitMqConfigurations.Configurations.ForEach((configuration) => services.AddKeyedSingleton(configuration.HostConfiguration.Key, configuration.HostConfiguration));

            return services;
        }

        private static IServiceCollection AddModelFactory(this IServiceCollection services)
        {
            return services.AddSingleton<ModelFactory>();
        }

        private static IServiceCollection AddDefaultChannel(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var rabbitMqConfigurations = provider.GetRequiredService<IRabbitMqConfigurations>();
            var modelFactory = provider.GetRequiredService<ModelFactory>();

            var defaultConfiguration = rabbitMqConfigurations.Configurations.Single(c => c.IsDefault);

            services.AddSingleton(modelFactory.CreateChannel(defaultConfiguration, defaultConfiguration.HostConfiguration));
            services.AddSingleton(sp => sp.GetRequiredService<IChannel>().Model);

            return services;
        }

        private static IServiceCollection AddKeyedChannels(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var rabbitMqConfigurations = provider.GetRequiredService<IRabbitMqConfigurations>();
            var modelFactory = provider.GetRequiredService<ModelFactory>();

            rabbitMqConfigurations.Configurations.Where(c => c.IsDefault == false).ToList().ForEach((configuration) =>
                services.AddKeyedSingleton(configuration.HostConfiguration.Key, modelFactory.CreateChannel(configuration, configuration.HostConfiguration)));

            return services;
        }

        private static IServiceCollection AddDefaultEventBus(this IServiceCollection services)
        {
            return services.AddSingleton<IEventBus, RabbitMQBusManager>((sp) =>
            {
                return new RabbitMQBusManager(sp.GetRequiredService<IServiceProvider>(),
                    sp.GetRequiredService<IChannel>(),
                    sp.GetRequiredService<IHttpContextAccessor>());
            });
        }

        private static IServiceCollection AddKeyedEventBuses(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var rabbitMqConfigurations = provider.GetRequiredService<IRabbitMqConfigurations>();
            rabbitMqConfigurations.Configurations.Where(c => c.IsDefault == false).ToList().ForEach((Configuration) => services.AddKeyedEventBus(Configuration.HostConfiguration.Key));

            return services;
        }

        private static IServiceCollection AddKeyedEventBus(this IServiceCollection services, string serviceKey)
        {
            return services.AddKeyedSingleton<IEventBus, RabbitMQBusManager>(serviceKey, (sp, key) =>
            {
                return new RabbitMQBusManager(serviceKey,
                    sp.GetRequiredService<IServiceProvider>(),
                   sp.GetRequiredService<IHttpContextAccessor>());
            });
        }
    }
}
