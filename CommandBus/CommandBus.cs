using Azure.ServiceBus.CommandBus;
using Azure.ServiceBus.CommandBus.Messages;
using CommandBus.Messages;
using CommandBus.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommandBus
{
    public class CommandBus : ICommandBus
	{
        private readonly ICommandBusSender _sender;
        private readonly ICommandBusProcessor _processor;
		private readonly IHandlerRegistry _handlerRegistry;
		private readonly ILogger<CommandBus> _logger;
		private readonly object _registrationLock = new object();
		private readonly IServiceProvider _serviceProvider;

		public CommandBus(ICommandBusSender sender,
            ICommandBusProcessor processor,
			IHandlerRegistry handlerRegistry,
			ILogger<CommandBus> logger,
			IServiceProvider serviceProvider)
        {
            _sender = sender;
            _processor = processor;
			_handlerRegistry = handlerRegistry;
			_logger = logger;
			_serviceProvider = serviceProvider;
		}

		public async Task<CommandResponseMessage> SendAsync<T>(T command, string queueName) where T : Command
		{
			var commandName = GetCommandName<T>();
			_logger.LogInformation("Publishing {commandType}", commandName);
			var message = Newtonsoft.Json.JsonConvert.SerializeObject(command);
			var commandMessage = new CommandMessage() { Body = message, CommandType = commandName };
			var json = JsonSerializer.Serialize(commandMessage);
			return await _sender.SendAsync(json, queueName);
		}

		public void RegisterHandler<T, TH>()
			where T : Command
			where TH : ICommandHandler<T>
		{
			var commandName = GetCommandName<T>();

			var containsKey = _handlerRegistry.HasRegistration<T>();
			if (containsKey)
			{
				var error = $"The messaging entity {commandName} already exists";
				_logger.LogWarning(error);
				throw new InvalidOperationException(error);
			}

			_logger.LogInformation("Registering {handlerType} for command {commandName}", typeof(TH), commandName);

			bool registerHandler = false;
			lock (_registrationLock)
			{
				if (_handlerRegistry.IsEmpty)
				{
					registerHandler = true;
				}
				_handlerRegistry.Register<T, TH>();
			}
			if (registerHandler)
			{
				RegisterHandler();
			}
		}

		public async Task StartProcessingAsync()
        {
			await _processor.StartProcessingAsync();
        }

		public async Task StopProcessingAsync()
		{
			await _processor.StopProcessingAsync();
		}

		private void RegisterHandler()
		{
			_processor.RegisterHandler(async (message) =>
			{
				var commandMessage = JsonSerializer.Deserialize<CommandMessage>(message);
				await ProcessCommand(commandMessage.CommandType, commandMessage.Body);
			});
		}

		private async Task<bool> ProcessCommand(string commandName, string message)
		{
			var processed = false;
			if (_handlerRegistry.HasRegistration(commandName))
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var registrations = _handlerRegistry.GetRegistrations(commandName);
					foreach (var registration in registrations)
					{
						var handler = scope.ServiceProvider.GetRequiredService(registration.Type);
						if (handler == null) continue;
						var eventType = _handlerRegistry.GetCommandTypeByName(commandName);
						var command = (Command)Newtonsoft.Json.JsonConvert.DeserializeObject(message, eventType);
						await (Task)registration.MethodInfo.Invoke(handler, new[] { command });
					}
				}
				processed = true;
			}
			return processed;
		}

		private string GetCommandName<T>()
		{
			return GetCommandName(typeof(T));
		}

		private string GetCommandName(Type type)
		{
			return type.Name;
		}

        public async ValueTask DisposeAsync()
        {
			if(_sender != null)
            {
				await _sender.DisposeAsync();
			}
			if (_processor != null)
			{
				await _processor.DisposeAsync();
			}
		}
    }
}
