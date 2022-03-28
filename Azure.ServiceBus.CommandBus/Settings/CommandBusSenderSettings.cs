
namespace Azure.ServiceBus.CommandBus.Settings
{
    public class CommandBusSenderSettings
    {
        /// <summary>
        /// The policy for this connection string must have the 'Manage' claim on the service bus if CreateQueue=True.
        /// Otherwise the policy must have the 'Send' and 'Listen' claims on the service bus.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Create the processor queue if it does not exist
        /// </summary>
        public bool CreateQueue { get; set; }

        /// <summary>
        /// The queue to reply to
        /// </summary>
        public string ReplyQueueName { get; set; }

        /// <summary>
        /// Default request timeout in seconds
        /// </summary>
        public int DefaultTimeoutSeconds { get; set; }
    }
}
