
namespace Azure.ServiceBus.CommandBus
{
    public interface ICommandBusProcessor : IAsyncDisposable
    {
        void RegisterHandler(Func<string, Task> handler);
        Task StartProcessingAsync();
        Task StopProcessingAsync();
    }
}
