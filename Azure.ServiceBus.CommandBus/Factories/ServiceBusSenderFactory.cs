using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Azure.ServiceBus.CommandBus.Factories
{
    internal class ServiceBusSenderFactory : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly bool _autoCreateQueue;
        private readonly string _adminConnectionString;
        private readonly object _lock = new object();
        private readonly Dictionary<string, ServiceBusSender> _senders = new Dictionary<string, ServiceBusSender>();

        internal ServiceBusSenderFactory(ServiceBusClient client,
            string adminConnectionString,
            bool autoCreateQueue)
        {
            _client = client;
            _adminConnectionString = adminConnectionString;
            _autoCreateQueue = autoCreateQueue;
        }

        internal ServiceBusSender Create(string queueName)
        {
            lock (_lock)
            {
                if(!_senders.TryGetValue(queueName, out ServiceBusSender sender))
                {
                    if (_autoCreateQueue)
                    {
                        CreateIfNotExists(queueName);
                    }
                    sender = _client.CreateSender(queueName);
                    _senders.Add(queueName, sender);
                }
                return sender;
            }
        }

        private void CreateIfNotExists(string queueName)
        {
            var adminClient = new ServiceBusAdministrationClient(_adminConnectionString);
            if (!adminClient.QueueExistsAsync(queueName).ConfigureAwait(true).GetAwaiter().GetResult())
            {
                var createQueueOptions = new CreateQueueOptions(queueName)
                {
                    MaxDeliveryCount = 1,
                    DefaultMessageTimeToLive = TimeSpan.FromSeconds(120),
                    LockDuration = TimeSpan.FromSeconds(30),
                    DeadLetteringOnMessageExpiration = false
                };
                adminClient.CreateQueueAsync(createQueueOptions).ConfigureAwait(true).GetAwaiter().GetResult();
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach(var sender in _senders.Values)
            {
                await sender.DisposeAsync();
            }
        }
    }
}
