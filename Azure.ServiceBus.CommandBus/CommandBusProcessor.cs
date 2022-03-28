using Azure.Messaging.ServiceBus;
using Azure.ServiceBus.CommandBus.Exceptions;
using Azure.ServiceBus.CommandBus.Messages;
using Azure.ServiceBus.CommandBus.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Azure.ServiceBus.CommandBus
{
    public class CommandBusProcessor : ICommandBusProcessor
    {
        private readonly ServiceBusClient _receiverClient;
        private readonly ServiceBusClient _replyClient;
        private readonly string _replyQueueName;
        private readonly ServiceBusProcessorOptions _receiverOptions;
        private readonly Lazy<ServiceBusProcessor> _processor;
        private readonly Lazy<ServiceBusSender> _replySender;
        private readonly string _queueName;
        private readonly ILogger<CommandBusProcessor> _logger;
        private readonly List<Func<string, Task>> _handlers = new List<Func<string, Task>>();

        internal CommandBusProcessor(IOptions<CommandBusProcessorSettings> options,
            ILogger<CommandBusProcessor> logger)
        {
            _receiverClient = new ServiceBusClient(options.Value.ConnectionString);
            _replyClient = new ServiceBusClient(options.Value.ReplyConnectionString);
            _queueName = options.Value.QueueName;
            _replyQueueName = options.Value.ReplyQueueName;
            _receiverOptions = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = true,
                MaxConcurrentCalls = options.Value.MaxConcurrentCommands,
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            };
            _processor = new Lazy<ServiceBusProcessor>(CreateServiceBusProcessor);
            _replySender = new Lazy<ServiceBusSender>(CreateServiceBusReplySender);
            _logger = logger;
        }

        private ServiceBusProcessor CreateServiceBusProcessor()
        {
            var processor = _receiverClient.CreateProcessor(_queueName, _receiverOptions);
            processor.ProcessMessageAsync += ReceiverMessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;
            return processor;
        }

        private ServiceBusSender CreateServiceBusReplySender()
        {
            return _replyClient.CreateSender(_replyQueueName);
        }

        public void RegisterHandler(Func<string, Task> handler)
        {
            _handlers.Add(handler);
        }

        public async Task StartProcessingAsync()
        {
            if (!_processor.Value.IsProcessing)
            {
                _logger.LogInformation("Listening for commands on: {queueName}", _queueName);
                await _processor.Value.StartProcessingAsync();
            }
        }

        public async Task StopProcessingAsync()
        {
            if (_processor.Value.IsProcessing)
            {
                _logger.LogInformation("Stopping listening for commands on: {queueName}", _queueName);
                await _processor.Value.StopProcessingAsync();
            }
        }

        private async Task ReceiverMessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            _logger.LogDebug("Processing command for session: {sessionId}", args.Message.ReplyToSessionId);

            try
            {
                if (_handlers.Count == 0)
                {
                    _logger.LogWarning("No command handler has been registered");
                }
                else
                {
                    foreach (var handler in _handlers)
                    {
                        await handler.Invoke(body);
                    }
                }
                await SendReplyAsync(new CommandResponseMessage("OK",CommandStatusCode.OK), args.Message.ReplyToSessionId);
            }
            catch(CommandValidationException ex)
            {
                _logger.LogError("Error processing command for session: {sessionId} - {ex}", args.Message.ReplyToSessionId, ex.Message);
                await SendReplyAsync(new CommandResponseMessage(ex.Message, CommandStatusCode.BadRequest), args.Message.ReplyToSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing command for session: {sessionId} - {ex}", args.Message.ReplyToSessionId, ex.ToString());
                await SendReplyAsync(new CommandResponseMessage(ex.ToString(), CommandStatusCode.InternalServerError), args.Message.ReplyToSessionId);
            }
        }

        private async Task SendReplyAsync(CommandResponseMessage response, string sessionId)
        {
            var message = GetResponseBody(response);
            var messageBatch = await _replySender.Value.CreateMessageBatchAsync();
            messageBatch.TryAddMessage(
                new ServiceBusMessage(message)
                {
                    SessionId = sessionId
                });
            _logger.LogDebug("Sending response for session: {sessionId}", sessionId);
            await _replySender.Value.SendMessagesAsync(messageBatch);
        }

        private string GetResponseBody(CommandResponseMessage response)
        {
            return JsonSerializer.Serialize(response);
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError("Error processing command - {ex}", args.Exception.ToString());
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_processor.IsValueCreated)
            {
                await StopProcessingAsync();
                await _processor.Value.DisposeAsync();
            }
            if (_replySender.IsValueCreated)
            {
                await _replySender.Value.DisposeAsync();
            }
            await _receiverClient.DisposeAsync();
            await _replyClient.DisposeAsync();
        }
    }
}
