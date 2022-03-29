using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CommandBus.Builders
{
    public interface ICommandBusBuilder
    {
        IServiceCollection Services { get; }
        ICommandBusBuilder AddSender(Action<CommandBusSenderSettings> configureSettings);
        ICommandBusBuilder AddProcessor(Action<CommandBusProcessorSettings> configureSettings);
        ICommandBusBuilder AddHandlers(Assembly handlerAssembley);
        ICommandBusBuilder AddHandlers(Assembly[] handlerAssemblies);
    }
}
