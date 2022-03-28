using Azure.ServiceBus.CommandBus.Messages;

namespace Azure.ServiceBus.CommandBus.Exceptions
{
    public class CommandValidationException : CommandException
    {
        public CommandValidationException(string validationMessage) : base(validationMessage, (int)CommandStatusCode.BadRequest)
        {
        }
    }
}
