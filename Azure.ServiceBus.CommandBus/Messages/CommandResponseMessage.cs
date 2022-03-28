using Azure.ServiceBus.CommandBus.Exceptions;

namespace Azure.ServiceBus.CommandBus.Messages
{
    public class CommandResponseMessage
    {
        public CommandResponseMessage(string content, CommandStatusCode statusCode)
        {
            Content = content;
            StatusCode = statusCode;
        }

        public string Content { get; set; }
        public CommandStatusCode StatusCode { get; set; }

        public override string ToString()
        {
            return Content;
        }

        public bool IsSuccessStatusCode()
        {
            return StatusCode == CommandStatusCode.OK;
        }

        public void EnsureSuccessStatusCode()
        {
            if(!IsSuccessStatusCode())
            {
                if(StatusCode == CommandStatusCode.BadRequest)
                {
                    throw new CommandValidationException(Content);
                }
                else
                {
                    throw new CommandException(Content, (int)StatusCode);
                }
            }
        }
    }
}