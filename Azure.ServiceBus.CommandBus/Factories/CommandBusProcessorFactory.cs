using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.ServiceBus.CommandBus.Factories
{
    public class CommandBusProcessorFactory : ICommandBusProcessorFactory
    {
        private readonly IOptions<CommandBusProcessorSettings> _options;
        private readonly ILogger<CommandBusProcessor> _logger;

        public CommandBusProcessorFactory(IOptions<CommandBusProcessorSettings> options,
            ILogger<CommandBusProcessor> logger)
        {
            _options = options;
            _logger = logger;
        }

        public ICommandBusProcessor Create()
        {
            return new CommandBusProcessor(_options, _logger);
        }
    }
}
