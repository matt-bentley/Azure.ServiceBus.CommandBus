
namespace CommandBus.Subscriptions
{
	public interface IHandlerRegistry
	{
		bool IsEmpty { get; }

		void Register<T, TH>()
			where T : Command
			where TH : ICommandHandler<T>;

		bool HasRegistration<T>() where T : Command;
		bool HasRegistration(string commandName);
		Type GetCommandTypeByName(string commandName);
		IEnumerable<HandlerRegistration> GetRegistrations(string commandName);
	}
}
