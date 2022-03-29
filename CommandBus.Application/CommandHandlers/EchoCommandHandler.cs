using Azure.ServiceBus.CommandBus.Exceptions;
using CommandBus.Application.Commands;
using Microsoft.Extensions.Logging;

namespace CommandBus.Application.CommandHandlers
{
    public class EchoCommandHandler : ICommandHandler<EchoCommand>
    {
        private readonly ILogger<EchoCommandHandler> _logger;

        public EchoCommandHandler(ILogger<EchoCommandHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(EchoCommand command)
        {
            _logger.LogInformation("Processing message: {message}", command.Message);
            if (command.Message == "Error")
            {
                throw new ApplicationException("There was an error");
            }
            if (command.Message == "Hello 10!")
            {
                throw new CommandValidationException($"Invalid message: {command.Message}");
            }
            return Task.CompletedTask;
        }
    }
}
