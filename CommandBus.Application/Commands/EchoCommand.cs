
namespace CommandBus.Application.Commands
{
    public class EchoCommand : Command
    {
        public EchoCommand(string message, string correlationId) : base(correlationId)
        {
            Message = message;
        }

        public readonly string Message;
    }
}
