using Azure.ServiceBus.CommandBus;
using CommandBus.Exceptions;
using CommandBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommandBus.Factories
{
    public class CommandBusFactory : ICommandBusFactory
    {
        private readonly ILogger<CommandBus> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CommandBusFactory(ILogger<CommandBus> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public ICommandBus Create()
        {
            var sender = _serviceProvider.GetService<ICommandBusSender>();
            var processor = _serviceProvider.GetService<ICommandBusProcessor>();

            if(sender == null && processor == null)
            {
                throw new RegistrationException($"No command sender or processor has been registered, use .AddCommandBusProcessor or .AddCommandBusSender to setup the Command Bus");
            }

            return new CommandBus(sender, processor, new InMemoryHandlerRegistry(), _logger, _serviceProvider);
        }
    }
}
