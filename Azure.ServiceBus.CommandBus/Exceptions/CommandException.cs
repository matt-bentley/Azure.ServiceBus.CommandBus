
namespace Azure.ServiceBus.CommandBus.Exceptions
{
    public class CommandException : Exception
    {
        public CommandException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public readonly int StatusCode;
    }
}
