namespace ChatService.Domain.Request
{
    public class ModerateMessageRequest : AddMessageRequest
    {
        public string Guid { get; set; }
        public DateTime CreatedAt { get; set; }
        public string MessageID { get; set; }
    }
}
