
namespace Azure.ServiceBus.CommandBus.Factories
{
    public interface ICommandBusSenderFactory
    {
        ICommandBusSender Create();
    }
}
