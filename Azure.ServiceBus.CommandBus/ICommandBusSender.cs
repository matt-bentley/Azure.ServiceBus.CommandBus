using Azure.ServiceBus.CommandBus.Messages;

namespace Azure.ServiceBus.CommandBus
{
    public interface ICommandBusSender : IAsyncDisposable
    {
        Task<CommandResponseMessage> SendAsync(string message, string queueName);
    }
}
