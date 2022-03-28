
namespace Azure.ServiceBus.CommandBus.Factories
{
    public interface ICommandBusProcessorFactory
    {
        ICommandBusProcessor Create();
    }
}
