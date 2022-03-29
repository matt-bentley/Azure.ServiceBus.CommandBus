
namespace CommandBus
{
	public interface ICommandHandler<in TCommand> : ICommandHandler
		where TCommand : Command
	{
		Task HandleAsync(TCommand command);
	}

	public interface ICommandHandler
	{
	}
}
