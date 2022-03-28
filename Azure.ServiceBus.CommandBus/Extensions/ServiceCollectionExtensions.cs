using Azure.ServiceBus.CommandBus.Factories;
using Azure.ServiceBus.CommandBus.Settings;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string DEFAULT_REPLY_QUEUE = "commandbus-reply";

        public static IServiceCollection AddCommandBusSender(this IServiceCollection services, Action<CommandBusSenderSettings> configureSettings)
        {
            var settings = new CommandBusSenderSettings()
            {
                CreateQueue = true,
                ReplyQueueName = DEFAULT_REPLY_QUEUE,
                DefaultTimeoutSeconds = 30
            };
            configureSettings.Invoke(settings);
            services.AddSingleton(Options.Options.Create(settings));

            services.AddSingleton<ICommandBusSenderFactory, CommandBusSenderFactory>();
            services.AddSingleton(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<ICommandBusSenderFactory>();
                return factory.Create();
            });
            return services;
        }

        public static IServiceCollection AddCommandBusProcessor(this IServiceCollection services, Action<CommandBusProcessorSettings> configureSettings)
        {
            var settings = new CommandBusProcessorSettings()
            {
                ReplyQueueName = DEFAULT_REPLY_QUEUE,
                MaxConcurrentCommands = 5
            };
            configureSettings.Invoke(settings);
            services.AddSingleton(Options.Options.Create(settings));

            services.AddSingleton<ICommandBusProcessorFactory, CommandBusProcessorFactory>();
            services.AddSingleton(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<ICommandBusProcessorFactory>();
                return factory.Create();
            });
            return services;
        }
    }
}
