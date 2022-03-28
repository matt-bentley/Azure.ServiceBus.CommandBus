
namespace Azure.ServiceBus.CommandBus.Settings
{
    public class CommandBusProcessorSettings
    {
        /// <summary>
        /// The policy for this connection string must have the 'Listen' claim on the service bus
        /// or the queue that is being listened to
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The policy for this connection string must have the 'Send' claim on the service bus 
        /// or the Reply queue
        /// </summary>
        public string ReplyConnectionString { get; set; }

        /// <summary>
        /// The queue to receive command from
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The queue to reply to
        /// </summary>
        public string ReplyQueueName { get; set; }

        /// <summary>
        /// The maximum number of commands that can be processed at the same time
        /// from this processor instance
        /// </summary>
        public int MaxConcurrentCommands { get; set; }
    }
}
