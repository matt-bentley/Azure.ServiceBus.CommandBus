
using Azure.ServiceBus.CommandBus.Messages;

namespace CommandBus
{
    public interface ICommandBus : IAsyncDisposable
    {
        Task<CommandResponseMessage> SendAsync<T>(T command, string queueName) where T : Command;
        void RegisterHandler<T, TH>()
            where T : Command
            where TH : ICommandHandler<T>;
        Task StartProcessingAsync();
        Task StopProcessingAsync();
    }
}
