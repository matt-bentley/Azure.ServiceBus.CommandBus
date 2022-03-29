using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CommandBus.Builders
{
    public class CommandBusBuilder : ICommandBusBuilder
    {
        private readonly IServiceCollection _services;

        public CommandBusBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IServiceCollection Services => _services;

        public ICommandBusBuilder AddSender(Action<CommandBusSenderSettings> configureSettings)
        {
            _services.AddCommandBusSender(configureSettings);
            return this;
        }

        public ICommandBusBuilder AddProcessor(Action<CommandBusProcessorSettings> configureSettings)
        {
            _services.AddCommandBusProcessor(configureSettings);
            return this;
        }

        public ICommandBusBuilder AddHandlers(Assembly handlerAssembley)
        {
            var handlerInterface = typeof(ICommandHandler);
            var handlerTypes = handlerAssembley.GetTypes().Where(e => handlerInterface.IsAssignableFrom(e) && !e.IsAbstract && !e.IsInterface);
            foreach(var handlerType in handlerTypes)
            {
                Services.AddTransient(handlerType);
            }
            return this;
        }

        public ICommandBusBuilder AddHandlers(Assembly[] handlerAssemblies)
        {
            foreach(var assembly in handlerAssemblies)
            {
                AddHandlers(assembly);
            }
            return this;
        }
    }
}
