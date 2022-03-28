using Azure.Messaging.ServiceBus;
using Azure.ServiceBus.CommandBus.Factories;
using Azure.ServiceBus.CommandBus.Messages;
using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Azure.ServiceBus.CommandBus
{
    public class CommandBusSender : ICommandBusSender
    {
        private readonly ServiceBusClient _client;
        private readonly string _replyQueueName;
        private readonly ServiceBusSenderFactory _senderFactory;
        private readonly ServiceBusSessionReceiverOptions _replyOptions;
        private readonly TimeSpan _replyTimeout;
        private readonly ILogger<CommandBusSender> _logger;

        internal CommandBusSender(IOptions<CommandBusSenderSettings> options,
            ILogger<CommandBusSender> logger)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
            _replyQueueName = options.Value.ReplyQueueName;
            _senderFactory = new ServiceBusSenderFactory(_client, options.Value.ConnectionString, options.Value.CreateQueue);
            _replyOptions = new ServiceBusSessionReceiverOptions()
            {
                PrefetchCount = 1,
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            };
            _replyTimeout = TimeSpan.FromSeconds(options.Value.DefaultTimeoutSeconds);
            _logger = logger;
        }

        public async Task<CommandResponseMessage> SendAsync(string message, string queueName)
        {
            var replySessionId = Guid.NewGuid().ToString();
            var sender = _senderFactory.Create(queueName);
            var session = await _client.AcceptSessionAsync(_replyQueueName, replySessionId, _replyOptions);

            var command = new ServiceBusMessage(message)
            {
                ReplyToSessionId = replySessionId
            };

            _logger.LogDebug("Sending command to: {queueName} with session: {replySessionId}", queueName, replySessionId);
            await sender.SendMessageAsync(command);

            var replyMessage = await session.ReceiveMessageAsync(_replyTimeout);
            _logger.LogDebug("Processing response for session: {replySessionId}", replySessionId);
            var replyBody = replyMessage.Body.ToString();
            await session.CloseAsync();
            return GetResponseMessage(replyBody);
        }

        private CommandResponseMessage GetResponseMessage(string replyBody)
        {
            return JsonSerializer.Deserialize<CommandResponseMessage>(replyBody);
        }

        public async ValueTask DisposeAsync()
        {
            await _senderFactory.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
