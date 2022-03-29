using CommandBus.Builders;
using CommandBus.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static ICommandBusBuilder AddCommandBus(this IServiceCollection services)
        {
            services.AddSingleton<ICommandBusFactory, CommandBusFactory>();
            services.AddSingleton(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<ICommandBusFactory>();
                return factory.Create();
            });
            return new CommandBusBuilder(services);
        }
    }
}
