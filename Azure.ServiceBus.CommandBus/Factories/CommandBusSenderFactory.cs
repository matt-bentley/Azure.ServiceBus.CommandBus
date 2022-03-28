using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Azure.ServiceBus.CommandBus.Factories
{
    public class CommandBusSenderFactory : ICommandBusSenderFactory
    {
        private readonly IOptions<CommandBusSenderSettings> _options;
        private readonly ILogger<CommandBusSender> _logger;

        public CommandBusSenderFactory(IOptions<CommandBusSenderSettings> options,
            ILogger<CommandBusSender> logger)
        {
            _options = options;
            _logger = logger;
        }

        public ICommandBusSender Create()
        {
            return new CommandBusSender(_options, _logger);
        }
    }
}
