
namespace CommandBus.Subscriptions
{
    public class InMemoryHandlerRegistry : IHandlerRegistry
    {
        private readonly Dictionary<string, List<HandlerRegistration>> _handlers;
        private readonly List<Type> _commandTypes;

        public InMemoryHandlerRegistry()
        {
            _handlers = new Dictionary<string, List<HandlerRegistration>>();
            _commandTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();

        public void Register<T, TH>()
            where T : Command
            where TH : ICommandHandler<T>
        {
            var commandName = GetCommandKey<T>();

            AddHandlerRegistration(typeof(TH), commandName);

            if (!_commandTypes.Contains(typeof(T)))
            {
                _commandTypes.Add(typeof(T));
            }
        }

        private void AddHandlerRegistration(Type handlerType, string commandName)
        {
            if (!HasRegistration(commandName))
            {
                _handlers.Add(commandName, new List<HandlerRegistration>());
            }

            var registration = new HandlerRegistration(handlerType);

            if (_handlers[commandName].Any(s => s.Equals(registration)))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already registered for '{commandName}'", nameof(handlerType));
            }

            _handlers[commandName].Add(registration);
        }

        private string GetCommandKey<T>()
        {
            return typeof(T).Name;
        }

        public Type GetCommandTypeByName(string commandName) => _commandTypes.SingleOrDefault(t => t.Name == commandName);

        public IEnumerable<HandlerRegistration> GetRegistrations(string commandName) => _handlers[commandName];

        public bool HasRegistration<T>() where T : Command
        {
            var key = GetCommandKey<T>();
            return HasRegistration(key);
        }

        public bool HasRegistration(string commandName) => _handlers.ContainsKey(commandName);
    }
}
