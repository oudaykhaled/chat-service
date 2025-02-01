namespace ChatService.Domain.Request
{
    public class EditMessageRequest : AddMessageRequest
    {
        public string MessageID { get; set; }
    }
}
