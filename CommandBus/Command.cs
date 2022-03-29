using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommandBus
{
	public abstract class Command
	{
		public Command(string correlationId) : this(Guid.NewGuid(), DateTime.UtcNow, correlationId)
		{

		}

		[JsonConstructor]
		public Command(Guid id, DateTime createDate, string correlationId)
		{
			Id = id;
			CreationDate = createDate;
			CorrelationId = correlationId;
		}

		[JsonProperty]
		public Guid Id { get; private set; }

		[JsonProperty]
		public string CorrelationId { get; private set; }

		[JsonProperty]
		public DateTime CreationDate { get; private set; }

		[JsonProperty]
		public Dictionary<string, object> EventState { get; } = new Dictionary<string, object>();

		[JsonExtensionData]
		public IDictionary<string, JToken> AdditionalData { get; set; }
	}
}
